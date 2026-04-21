using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// Ensures the database is migrated and seeded with a realistic demo dataset for Qadam.
    /// Safe to call at every startup: it only seeds when the Employees table is empty.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
        {
            await context.Database.MigrateAsync(cancellationToken);

            if (await context.Employees.AnyAsync(cancellationToken))
            {
                return;
            }

            // 1) Five team members — one Manager + four Workers
            var aizhan  = new Employee("Aizhan Abdrakhmanova", "aizhan@qadam.kg",  EmployeeRole.Manager);
            var azamat  = new Employee("Azamat Kasymov",        "azamat@qadam.kg",  EmployeeRole.Worker);
            var nurkyz  = new Employee("Nurkyz Beksultanova",   "nurkyz@qadam.kg",  EmployeeRole.Worker);
            var bekzhan = new Employee("Bekzhan Orozov",        "bekzhan@qadam.kg", EmployeeRole.Worker);
            var meerim  = new Employee("Meerim Tashmatova",     "meerim@qadam.kg",  EmployeeRole.Worker);
            context.Employees.AddRange(aizhan, azamat, nurkyz, bekzhan, meerim);

            // 2) Availability windows — realistic small-café staffing
            var availabilities = new List<Availability>
            {
                // Aizhan (Manager) — Mon/Wed/Fri 08:00–17:00
                new(aizhan.Id,  DayOfWeek.Monday,    T(8),  T(17)),
                new(aizhan.Id,  DayOfWeek.Wednesday, T(8),  T(17)),
                new(aizhan.Id,  DayOfWeek.Friday,    T(8),  T(17)),

                // Azamat — Tue/Thu/Fri 09:00–17:00, Sat 10:00–18:00
                new(azamat.Id,  DayOfWeek.Tuesday,   T(9),  T(17)),
                new(azamat.Id,  DayOfWeek.Thursday,  T(9),  T(17)),
                new(azamat.Id,  DayOfWeek.Friday,    T(9),  T(17)),
                new(azamat.Id,  DayOfWeek.Saturday,  T(10), T(18)),

                // Nurkyz — Mon/Tue/Thu 09:00–17:00, Sun 10:00–18:00
                new(nurkyz.Id,  DayOfWeek.Monday,    T(9),  T(17)),
                new(nurkyz.Id,  DayOfWeek.Tuesday,   T(9),  T(17)),
                new(nurkyz.Id,  DayOfWeek.Thursday,  T(9),  T(17)),
                new(nurkyz.Id,  DayOfWeek.Sunday,    T(10), T(18)),

                // Bekzhan — Mon–Fri 16:00–22:00 (evening closer)
                new(bekzhan.Id, DayOfWeek.Monday,    T(16), T(22)),
                new(bekzhan.Id, DayOfWeek.Tuesday,   T(16), T(22)),
                new(bekzhan.Id, DayOfWeek.Wednesday, T(16), T(22)),
                new(bekzhan.Id, DayOfWeek.Thursday,  T(16), T(22)),
                new(bekzhan.Id, DayOfWeek.Friday,    T(16), T(22)),

                // Meerim — Wed/Sat/Sun 10:00–18:00
                new(meerim.Id,  DayOfWeek.Wednesday, T(10), T(18)),
                new(meerim.Id,  DayOfWeek.Saturday,  T(10), T(18)),
                new(meerim.Id,  DayOfWeek.Sunday,    T(10), T(18)),
            };
            context.Availabilities.AddRange(availabilities);

            // 3) Shifts for the current week (Mon → Sun of today's week)
            var today = DateOnly.FromDateTime(DateTime.Today);
            var daysSinceMonday = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var monday = today.AddDays(-daysSinceMonday);

            // Current week
            var shiftAizhanMon = Assigned(monday,                T(8),  T(17), aizhan.Id);
            // Meerim's Monday evening shift — target of the Drop request
            var shiftMeerimMonEve = Assigned(monday,             T(16), T(22), meerim.Id);
            // Bekzhan also on Monday evening — concurrent coverage
            var shiftBekzhanMonEve = Assigned(monday,            T(17), T(22), bekzhan.Id);
            var shiftNurkyzTue = Assigned(monday.AddDays(1),     T(9),  T(17), nurkyz.Id);
            var shiftBekzhanWedEve = Assigned(monday.AddDays(2), T(16), T(22), bekzhan.Id);
            // Open Wednesday daytime shift — target of the Pickup request
            var shiftOpenWed = Open(monday.AddDays(2),           T(10), T(18));
            var shiftAzamatThu = Assigned(monday.AddDays(3),     T(9),  T(17), azamat.Id);
            var shiftAizhanFri = Assigned(monday.AddDays(4),     T(8),  T(17), aizhan.Id);
            // Saturday concurrent daytime — Azamat + Meerim same 10–18 window
            var shiftAzamatSat = Assigned(monday.AddDays(5),     T(10), T(18), azamat.Id);
            var shiftMeerimSat = Assigned(monday.AddDays(5),     T(10), T(18), meerim.Id);

            // Next week — Mon morning + evening, Sat concurrent daytime
            var nextMonday = monday.AddDays(7);
            var shiftAizhanNextMon     = Assigned(nextMonday,            T(8),  T(17), aizhan.Id);
            var shiftBekzhanNextMonEve = Assigned(nextMonday,            T(16), T(22), bekzhan.Id);
            var shiftNurkyzNextTue     = Assigned(nextMonday.AddDays(1), T(9),  T(17), nurkyz.Id);
            var shiftAzamatNextSat     = Assigned(nextMonday.AddDays(5), T(10), T(18), azamat.Id);
            var shiftMeerimNextSat     = Assigned(nextMonday.AddDays(5), T(10), T(18), meerim.Id);

            context.Shifts.AddRange(
                shiftAizhanMon, shiftMeerimMonEve, shiftBekzhanMonEve, shiftNurkyzTue,
                shiftBekzhanWedEve, shiftOpenWed, shiftAzamatThu, shiftAizhanFri,
                shiftAzamatSat, shiftMeerimSat,
                shiftAizhanNextMon, shiftBekzhanNextMonEve, shiftNurkyzNextTue,
                shiftAzamatNextSat, shiftMeerimNextSat);

            // 4) Two pending shift requests
            var pickupRequest = new ShiftRequest(
                shiftOpenWed.Id,
                azamat.Id,
                ShiftRequestType.Pickup,
                "Saving up for a new laptop, would love extra hours");

            var dropRequest = new ShiftRequest(
                shiftMeerimMonEve.Id,
                meerim.Id,
                ShiftRequestType.Drop,
                "Family event on Monday evening — need to drop");

            context.ShiftRequests.AddRange(pickupRequest, dropRequest);

            await context.SaveChangesAsync(cancellationToken);
        }

        private static TimeSpan T(int hour) => new(hour, 0, 0);

        private static Shift Assigned(DateOnly date, TimeSpan start, TimeSpan end, Guid employeeId)
        {
            var shift = new Shift(date, start, end);
            shift.AssignEmployee(employeeId);
            return shift;
        }

        private static Shift Open(DateOnly date, TimeSpan start, TimeSpan end)
        {
            var shift = new Shift(date, start, end);
            shift.OpenForPickup();
            return shift;
        }
    }
}

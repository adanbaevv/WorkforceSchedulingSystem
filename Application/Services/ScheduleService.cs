using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private const double WeeklyHoursCap = 40d;

        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IShiftRepository _shiftRepository;

        public ScheduleService(
            IEmployeeRepository employeeRepository,
            IAvailabilityRepository availabilityRepository,
            IShiftRepository shiftRepository)
        {
            _employeeRepository = employeeRepository;
            _availabilityRepository = availabilityRepository;
            _shiftRepository = shiftRepository;
        }

        /// <summary>
        /// Builds a deterministic, load-balanced schedule proposal for the given week of shift slots.
        /// Processes slots in (date, start-time, end-time) order. For each slot, eligible employees
        /// are those with an availability window on that day-of-week covering the slot and whose
        /// provisional weekly hours would not exceed the 40-hour cap. Ties are broken by lowest
        /// current total hours, then by employee identifier for determinism. Existing assigned
        /// shifts inside the target week count toward each employee's starting hours.
        /// </summary>
        /// <param name="weekStartDate">The first date of the target week (inclusive).</param>
        /// <param name="slots">The shift slots to fill for this week.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A proposal containing assignments, unassigned slots and per-employee hour totals.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        public async Task<ScheduleProposal> GenerateAsync(
            DateOnly weekStartDate,
            IReadOnlyList<ShiftSlot> slots,
            CancellationToken cancellationToken = default)
        {
            if (slots == null)
            {
                throw new ValidationException("Slots must be provided.");
            }

            var weekEndDate = weekStartDate.AddDays(6);

            foreach (var slot in slots)
            {
                if (slot.StartTime >= slot.EndTime)
                {
                    throw new ValidationException(
                        $"Slot on {slot.Date:yyyy-MM-dd} has start time >= end time.");
                }

                if (slot.Date < weekStartDate || slot.Date > weekEndDate)
                {
                    throw new ValidationException(
                        $"Slot date {slot.Date:yyyy-MM-dd} is outside the target week " +
                        $"[{weekStartDate:yyyy-MM-dd}..{weekEndDate:yyyy-MM-dd}].");
                }
            }

            var allEmployees = await _employeeRepository.GetAllAsync(cancellationToken);
            var employees = allEmployees
                .Where(employee => employee.IsActive)
                .OrderBy(employee => employee.Id)
                .ToList();

            var allAvailabilities = await _availabilityRepository.GetAllAsync(cancellationToken);
            var availabilitiesByEmployee = allAvailabilities
                .GroupBy(availability => availability.EmployeeId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var existingShifts = await _shiftRepository.GetByDateRangeAsync(weekStartDate, weekEndDate, cancellationToken);

            var existingHoursByEmployee = employees.ToDictionary(employee => employee.Id, _ => 0d);
            var existingShiftsByEmployeeByDate = new Dictionary<(Guid, DateOnly), List<(TimeSpan start, TimeSpan end)>>();

            foreach (var shift in existingShifts)
            {
                if (!shift.AssignedEmployeeId.HasValue || shift.Status != ShiftStatus.Assigned)
                {
                    continue;
                }

                var employeeId = shift.AssignedEmployeeId.Value;
                if (existingHoursByEmployee.ContainsKey(employeeId))
                {
                    existingHoursByEmployee[employeeId] += (shift.EndTime - shift.StartTime).TotalHours;
                }

                var key = (employeeId, shift.Date);
                if (!existingShiftsByEmployeeByDate.TryGetValue(key, out var list))
                {
                    list = new List<(TimeSpan, TimeSpan)>();
                    existingShiftsByEmployeeByDate[key] = list;
                }

                list.Add((shift.StartTime, shift.EndTime));
            }

            var provisionalHoursByEmployee = employees.ToDictionary(employee => employee.Id, _ => 0d);
            var provisionalShiftsByEmployeeByDate = new Dictionary<(Guid, DateOnly), List<(TimeSpan start, TimeSpan end)>>();

            var sortedSlots = slots
                .OrderBy(slot => slot.Date)
                .ThenBy(slot => slot.StartTime)
                .ThenBy(slot => slot.EndTime)
                .ToList();

            var assignments = new List<ScheduledAssignment>();
            var unassigned = new List<UnassignedSlot>();

            foreach (var slot in sortedSlots)
            {
                var slotHours = slot.DurationHours;
                var eligible = new List<Employee>();
                var unavailableCount = 0;
                var capReachedCount = 0;
                var overlapCount = 0;

                foreach (var employee in employees)
                {
                    if (!availabilitiesByEmployee.TryGetValue(employee.Id, out var availabilities))
                    {
                        unavailableCount++;
                        continue;
                    }

                    var covered = availabilities.Any(availability =>
                        availability.DayOfWeek == slot.Date.DayOfWeek &&
                        availability.StartTime <= slot.StartTime &&
                        availability.EndTime >= slot.EndTime);

                    if (!covered)
                    {
                        unavailableCount++;
                        continue;
                    }

                    var projectedHours = existingHoursByEmployee[employee.Id]
                                         + provisionalHoursByEmployee[employee.Id]
                                         + slotHours;

                    if (projectedHours > WeeklyHoursCap)
                    {
                        capReachedCount++;
                        continue;
                    }

                    if (HasSameDayOverlap(existingShiftsByEmployeeByDate, provisionalShiftsByEmployeeByDate, employee.Id, slot))
                    {
                        overlapCount++;
                        continue;
                    }

                    eligible.Add(employee);
                }

                if (eligible.Count == 0)
                {
                    unassigned.Add(new UnassignedSlot(
                        slot.Date,
                        slot.StartTime,
                        slot.EndTime,
                        BuildUnassignedReason(unavailableCount, capReachedCount, overlapCount, employees.Count)));
                    continue;
                }

                var chosen = eligible
                    .OrderBy(employee => existingHoursByEmployee[employee.Id] + provisionalHoursByEmployee[employee.Id])
                    .ThenBy(employee => employee.Id)
                    .First();

                assignments.Add(new ScheduledAssignment(
                    slot.Date,
                    slot.StartTime,
                    slot.EndTime,
                    chosen.Id,
                    chosen.FullName,
                    slotHours));

                provisionalHoursByEmployee[chosen.Id] += slotHours;

                var key = (chosen.Id, slot.Date);
                if (!provisionalShiftsByEmployeeByDate.TryGetValue(key, out var list))
                {
                    list = new List<(TimeSpan, TimeSpan)>();
                    provisionalShiftsByEmployeeByDate[key] = list;
                }

                list.Add((slot.StartTime, slot.EndTime));
            }

            var employeeHours = employees
                .Select(employee => new EmployeeScheduledHours(
                    employee.Id,
                    employee.FullName,
                    existingHoursByEmployee[employee.Id],
                    provisionalHoursByEmployee[employee.Id]))
                .ToList();

            return new ScheduleProposal(weekStartDate, weekEndDate, assignments, unassigned, employeeHours);
        }

        /// <summary>
        /// Materializes an approved schedule into persisted shift entities. Validates that every referenced
        /// employee exists and is active, and that no assigned slot overlaps an existing shift for the same
        /// employee or another assigned slot in the same request.
        /// </summary>
        /// <param name="slots">The approved slots to materialize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A summary of the commit, including the identifiers of all created shifts.</returns>
        /// <exception cref="ValidationException">Thrown when the input is invalid.</exception>
        /// <exception cref="NotFoundException">Thrown when a referenced employee does not exist.</exception>
        /// <exception cref="ConflictException">Thrown when a slot collides with an existing shift or another slot.</exception>
        public async Task<ScheduleCommitResult> CommitAsync(
            IReadOnlyList<CommitScheduleSlot> slots,
            CancellationToken cancellationToken = default)
        {
            if (slots == null || slots.Count == 0)
            {
                throw new ValidationException("At least one slot is required.");
            }

            foreach (var slot in slots)
            {
                if (slot.StartTime >= slot.EndTime)
                {
                    throw new ValidationException(
                        $"Slot on {slot.Date:yyyy-MM-dd} has start time >= end time.");
                }
            }

            var referencedEmployeeIds = slots
                .Where(slot => slot.AssignedEmployeeId.HasValue)
                .Select(slot => slot.AssignedEmployeeId!.Value)
                .Distinct()
                .ToList();

            foreach (var employeeId in referencedEmployeeIds)
            {
                var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
                if (employee == null)
                {
                    throw new NotFoundException($"Employee with id '{employeeId}' was not found.");
                }

                if (!employee.IsActive)
                {
                    throw new ValidationException($"Employee with id '{employeeId}' is inactive and cannot be assigned.");
                }
            }

            var assignedSlots = slots.Where(slot => slot.AssignedEmployeeId.HasValue).ToList();
            for (var i = 0; i < assignedSlots.Count; i++)
            {
                for (var j = i + 1; j < assignedSlots.Count; j++)
                {
                    var a = assignedSlots[i];
                    var b = assignedSlots[j];

                    if (a.AssignedEmployeeId != b.AssignedEmployeeId || a.Date != b.Date)
                    {
                        continue;
                    }

                    if (a.StartTime < b.EndTime && b.StartTime < a.EndTime)
                    {
                        throw new ConflictException(
                            $"Two slots for employee '{a.AssignedEmployeeId}' on {a.Date:yyyy-MM-dd} overlap.");
                    }
                }
            }

            foreach (var employeeId in referencedEmployeeIds)
            {
                var existingForEmployee = await _shiftRepository.GetByEmployeeAsync(employeeId, cancellationToken);
                var existingByDate = existingForEmployee
                    .GroupBy(shift => shift.Date)
                    .ToDictionary(group => group.Key, group => group.ToList());

                foreach (var slot in assignedSlots.Where(candidate => candidate.AssignedEmployeeId == employeeId))
                {
                    if (!existingByDate.TryGetValue(slot.Date, out var shiftsOnThatDate))
                    {
                        continue;
                    }

                    foreach (var existingShift in shiftsOnThatDate)
                    {
                        if (existingShift.StartTime < slot.EndTime && slot.StartTime < existingShift.EndTime)
                        {
                            throw new ConflictException(
                                $"Slot on {slot.Date:yyyy-MM-dd} {slot.StartTime}-{slot.EndTime} " +
                                $"overlaps existing shift for employee '{employeeId}'.");
                        }
                    }
                }
            }

            var createdShifts = new List<Shift>();
            var assignedShiftsCreated = 0;
            var openShiftsCreated = 0;

            foreach (var slot in slots)
            {
                var shift = new Shift(slot.Date, slot.StartTime, slot.EndTime);
                if (slot.AssignedEmployeeId.HasValue)
                {
                    shift.AssignEmployee(slot.AssignedEmployeeId.Value);
                    assignedShiftsCreated++;
                }
                else
                {
                    shift.OpenForPickup();
                    openShiftsCreated++;
                }

                createdShifts.Add(shift);
            }

            await _shiftRepository.AddRangeAsync(createdShifts, cancellationToken);

            return new ScheduleCommitResult(
                assignedShiftsCreated,
                openShiftsCreated,
                createdShifts.Select(shift => shift.Id).ToList());
        }

        private static bool HasSameDayOverlap(
            Dictionary<(Guid, DateOnly), List<(TimeSpan start, TimeSpan end)>> existing,
            Dictionary<(Guid, DateOnly), List<(TimeSpan start, TimeSpan end)>> provisional,
            Guid employeeId,
            ShiftSlot slot)
        {
            var key = (employeeId, slot.Date);

            if (existing.TryGetValue(key, out var existingList))
            {
                foreach (var window in existingList)
                {
                    if (window.start < slot.EndTime && slot.StartTime < window.end)
                    {
                        return true;
                    }
                }
            }

            if (provisional.TryGetValue(key, out var provisionalList))
            {
                foreach (var window in provisionalList)
                {
                    if (window.start < slot.EndTime && slot.StartTime < window.end)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string BuildUnassignedReason(int unavailable, int capReached, int overlap, int totalEmployees)
        {
            if (totalEmployees == 0)
            {
                return "No active employees exist.";
            }

            if (unavailable == totalEmployees)
            {
                return "No employee has a matching availability window.";
            }

            if (capReached > 0 && overlap == 0 && unavailable + capReached == totalEmployees)
            {
                return "All available employees have reached the 40-hour weekly cap.";
            }

            return "No employee satisfied all constraints (availability, 40-hour cap, no same-day overlap).";
        }
    }
}

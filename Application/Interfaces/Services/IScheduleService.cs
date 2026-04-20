using Application.Common.Models;

namespace Application.Interfaces.Services
{
    public interface IScheduleService
    {
        /// <summary>
        /// Builds a deterministic, load-balanced schedule proposal for the given week of shift slots.
        /// Existing assigned shifts inside the same week are respected and counted toward each employee's
        /// weekly hours. No database writes are performed.
        /// </summary>
        /// <param name="weekStartDate">The first date of the target week (inclusive).</param>
        /// <param name="slots">The shift slots to fill for this week.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A proposal containing assignments, unassigned slots and per-employee hour totals.</returns>
        Task<ScheduleProposal> GenerateAsync(
            DateOnly weekStartDate,
            IReadOnlyList<ShiftSlot> slots,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Materializes an approved schedule into persisted shift entities.
        /// Each slot with an assigned employee becomes an assigned shift; slots without an employee become open-for-pickup shifts.
        /// </summary>
        /// <param name="slots">The approved slots to materialize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A summary of the commit, including the identifiers of all created shifts.</returns>
        Task<ScheduleCommitResult> CommitAsync(
            IReadOnlyList<CommitScheduleSlot> slots,
            CancellationToken cancellationToken = default);
    }
}

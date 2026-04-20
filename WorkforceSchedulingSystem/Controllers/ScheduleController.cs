using Asp.Versioning;
using Application.Common.Models;
using Application.Interfaces.Services;
using API.Dtos.Schedule;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// Generates a deterministic, load-balanced schedule proposal for a week of shift slots.
        /// The proposal is computed in memory; no shifts are written to the database.
        /// </summary>
        /// <param name="dto">The generation payload: week start date and shift slots.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A proposal containing assignments, unassigned slots and per-employee hour totals.</returns>
        [HttpPost("generate")]
        public async Task<ActionResult<ScheduleProposalDto>> Generate(
            [FromBody] GenerateScheduleDto dto,
            CancellationToken cancellationToken)
        {
            var slots = dto.Slots
                .Select(slot => new ShiftSlot(slot.Date, slot.StartTime, slot.EndTime))
                .ToList();

            var proposal = await _scheduleService.GenerateAsync(dto.WeekStartDate, slots, cancellationToken);

            return Ok(MapToDto(proposal));
        }

        /// <summary>
        /// Commits an approved schedule, creating a shift entity per slot. Slots with an assigned
        /// employee become assigned shifts; slots without an employee become open-for-pickup shifts.
        /// </summary>
        /// <param name="dto">The approved slots to materialize.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A summary of the commit operation, including identifiers of all created shifts.</returns>
        [HttpPost("commit")]
        public async Task<ActionResult<ScheduleCommitResultDto>> Commit(
            [FromBody] CommitScheduleDto dto,
            CancellationToken cancellationToken)
        {
            var slots = dto.Slots
                .Select(slot => new CommitScheduleSlot(slot.Date, slot.StartTime, slot.EndTime, slot.AssignedEmployeeId))
                .ToList();

            var result = await _scheduleService.CommitAsync(slots, cancellationToken);

            return Ok(new ScheduleCommitResultDto
            {
                AssignedShiftsCreated = result.AssignedShiftsCreated,
                OpenShiftsCreated = result.OpenShiftsCreated,
                CreatedShiftIds = result.CreatedShiftIds.ToList()
            });
        }

        private static ScheduleProposalDto MapToDto(ScheduleProposal proposal)
        {
            return new ScheduleProposalDto
            {
                WeekStartDate = proposal.WeekStartDate,
                WeekEndDate = proposal.WeekEndDate,
                Assignments = proposal.Assignments
                    .Select(assignment => new ScheduleAssignmentDto
                    {
                        Date = assignment.Date,
                        StartTime = assignment.StartTime,
                        EndTime = assignment.EndTime,
                        EmployeeId = assignment.EmployeeId,
                        EmployeeName = assignment.EmployeeName,
                        DurationHours = assignment.DurationHours
                    })
                    .ToList(),
                UnassignedSlots = proposal.UnassignedSlots
                    .Select(slot => new UnassignedSlotDto
                    {
                        Date = slot.Date,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Reason = slot.Reason
                    })
                    .ToList(),
                EmployeeHours = proposal.EmployeeHours
                    .Select(hours => new EmployeeHoursDto
                    {
                        EmployeeId = hours.EmployeeId,
                        EmployeeName = hours.EmployeeName,
                        ExistingHours = hours.ExistingHours,
                        NewlyAssignedHours = hours.NewlyAssignedHours,
                        TotalHours = hours.TotalHours
                    })
                    .ToList()
            };
        }
    }
}

using ETicket.Domain.Common;

namespace ETicket.Domain.Models;

public sealed record StatisticsFilter
{
    public StatisticsFilter(
        DateOnly from,
        DateOnly to,
        Guid? checkpointId,
        string? category,
        string? vehicleType)
    {
        if (to < from)
        {
            throw new DomainRuleViolationException(
                "statistics.invalid_period",
                "The statistics period end must not precede its start.");
        }

        if (to.DayNumber - from.DayNumber > 180)
        {
            throw new DomainRuleViolationException(
                "statistics.period_too_long",
                "The statistics period must not exceed 180 days.");
        }

        From = from;
        To = to;
        CheckpointId = checkpointId;
        Category = category;
        VehicleType = vehicleType;
    }

    public DateOnly From { get; }

    public DateOnly To { get; }

    public Guid? CheckpointId { get; }

    public string? Category { get; }

    public string? VehicleType { get; }
}

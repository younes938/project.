using System.Collections.Frozen;
using ETicket.Domain.Common;
using ETicket.Domain.Enums;

namespace ETicket.Domain.Services;

public sealed class VehicleStateTransitionService
{
    private static readonly FrozenSet<VehicleVisitStatus> EmptyTargets =
        Array.Empty<VehicleVisitStatus>().ToFrozenSet();

    private static readonly FrozenDictionary<VehicleVisitStatus, FrozenSet<VehicleVisitStatus>>
        AllowedTransitions = new Dictionary<VehicleVisitStatus, VehicleVisitStatus[]>
        {
            [VehicleVisitStatus.EntryDenied] = [],
            [VehicleVisitStatus.AwaitingControls] =
            [
                VehicleVisitStatus.ControlFailed,
                VehicleVisitStatus.AwaitingExit,
                VehicleVisitStatus.Cancelled
            ],
            [VehicleVisitStatus.ControlFailed] =
            [
                VehicleVisitStatus.Return,
                VehicleVisitStatus.Cancelled
            ],
            [VehicleVisitStatus.AwaitingExit] =
            [
                VehicleVisitStatus.Passed,
                VehicleVisitStatus.Return,
                VehicleVisitStatus.Cancelled
            ],
            [VehicleVisitStatus.Passed] = [],
            [VehicleVisitStatus.Return] = [],
            [VehicleVisitStatus.Cancelled] = []
        }.ToFrozenDictionary(
            entry => entry.Key,
            entry => entry.Value.ToFrozenSet());

    public bool CanTransition(VehicleVisitStatus from, VehicleVisitStatus to)
    {
        return AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public bool IsTerminal(VehicleVisitStatus status)
    {
        return AllowedTransitions.TryGetValue(status, out var targets) && targets.Count == 0;
    }

    public IReadOnlySet<VehicleVisitStatus> GetAllowedTargets(VehicleVisitStatus status)
    {
        return AllowedTransitions.TryGetValue(status, out var targets)
            ? targets
            : EmptyTargets;
    }

    public VehicleVisitStatus Transition(VehicleVisitStatus from, VehicleVisitStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new DomainRuleViolationException(
                "vehicle_visit.invalid_transition",
                $"Vehicle visit cannot transition from {from} to {to}.");
        }

        return to;
    }
}

using ETicket.Domain.Common;
using ETicket.Domain.Enums;
using ETicket.Domain.Services;

namespace ETicket.Domain.Tests;

public sealed class VehicleStateTransitionServiceTests
{
    private readonly VehicleStateTransitionService service = new();

    [Theory]
    [InlineData(VehicleVisitStatus.AwaitingControls, VehicleVisitStatus.ControlFailed)]
    [InlineData(VehicleVisitStatus.AwaitingControls, VehicleVisitStatus.AwaitingExit)]
    [InlineData(VehicleVisitStatus.AwaitingControls, VehicleVisitStatus.Cancelled)]
    [InlineData(VehicleVisitStatus.ControlFailed, VehicleVisitStatus.Return)]
    [InlineData(VehicleVisitStatus.ControlFailed, VehicleVisitStatus.Cancelled)]
    [InlineData(VehicleVisitStatus.AwaitingExit, VehicleVisitStatus.Passed)]
    [InlineData(VehicleVisitStatus.AwaitingExit, VehicleVisitStatus.Return)]
    [InlineData(VehicleVisitStatus.AwaitingExit, VehicleVisitStatus.Cancelled)]
    public void Transition_AllowsDocumentedStateChanges(
        VehicleVisitStatus from,
        VehicleVisitStatus to)
    {
        Assert.Equal(to, service.Transition(from, to));
    }

    [Theory]
    [InlineData(VehicleVisitStatus.AwaitingControls, VehicleVisitStatus.Passed)]
    [InlineData(VehicleVisitStatus.ControlFailed, VehicleVisitStatus.AwaitingExit)]
    [InlineData(VehicleVisitStatus.EntryDenied, VehicleVisitStatus.AwaitingControls)]
    [InlineData(VehicleVisitStatus.Passed, VehicleVisitStatus.Cancelled)]
    public void Transition_RejectsIllegalStateChanges(
        VehicleVisitStatus from,
        VehicleVisitStatus to)
    {
        var exception = Assert.Throws<DomainRuleViolationException>(
            () => service.Transition(from, to));

        Assert.Equal("vehicle_visit.invalid_transition", exception.Code);
    }

    [Theory]
    [InlineData(VehicleVisitStatus.EntryDenied)]
    [InlineData(VehicleVisitStatus.Passed)]
    [InlineData(VehicleVisitStatus.Return)]
    [InlineData(VehicleVisitStatus.Cancelled)]
    public void IsTerminal_IdentifiesTerminalStates(VehicleVisitStatus status)
    {
        Assert.True(service.IsTerminal(status));
        Assert.Empty(service.GetAllowedTargets(status));
    }
}

using ETicket.Domain.Enums;
using ETicket.Domain.Services;

namespace ETicket.Domain.Tests;

public sealed class VehicleEntryValidationServiceTests
{
    private readonly VehicleEntryValidationService service = new();

    [Fact]
    public void Validate_NormalizesARegularVehicleEntry()
    {
        var result = service.Validate(new VehicleEntryDraft(
            " aa-1234 bb ",
            "  Volvo ",
            " Truck ",
            false,
            2,
            [" bb 9876 aa ", null],
            " photo-1 "));

        Assert.True(result.IsValid);
        Assert.Equal("AA1234BB", result.Value!.PlateNumber);
        Assert.Equal("Volvo", result.Value.Brand);
        Assert.Equal(2, result.Value.PersonsCount);
        Assert.Equal(["BB9876AA"], result.Value.TrailerPlateNumbers);
        Assert.Equal("photo-1", result.Value.PhotoReference);
    }

    [Fact]
    public void Validate_RejectsPersonsAndTrailersForServiceVehicle()
    {
        var result = service.Validate(new VehicleEntryDraft(
            "AA1234BB",
            "Ford",
            "Service",
            true,
            1,
            ["BB1234AA"],
            null));

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "vehicle_entry.service_vehicle_persons_not_allowed");
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "vehicle_entry.service_vehicle_trailers_not_allowed");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_RejectsInvalidPersonsCountForRegularVehicle(int? personsCount)
    {
        var result = service.Validate(new VehicleEntryDraft(
            "AA1234BB",
            "Volvo",
            "Truck",
            false,
            personsCount,
            [],
            null));

        Assert.Contains(
            result.Issues,
            issue => issue.Code == "vehicle_entry.invalid_persons_count");
    }

    [Fact]
    public void Validate_RejectsDuplicateVehicleAndTrailerPlate()
    {
        var result = service.Validate(new VehicleEntryDraft(
            "AA1234BB",
            "Volvo",
            "Truck",
            false,
            1,
            ["AA-1234-BB"],
            null));

        Assert.Contains(
            result.Issues,
            issue => issue.Code == "vehicle_entry.duplicate_plate");
    }

    [Theory]
    [InlineData(GateMode.RearExit, true)]
    [InlineData(GateMode.BorderEntry, true)]
    [InlineData(GateMode.RearEntry, false)]
    [InlineData(GateMode.BorderExit, false)]
    public void SupportsEntry_UsesInboundCheckpointGates(GateMode mode, bool expected)
    {
        Assert.Equal(expected, service.SupportsEntry(mode));
    }
}

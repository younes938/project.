using System.Globalization;
using System.Text;
using ETicket.Domain.Enums;

namespace ETicket.Domain.Services;

public sealed record VehicleEntryDraft(
    string? PlateNumber,
    string? Brand,
    string? Type,
    bool IsServiceVehicle,
    int? PersonsCount,
    IReadOnlyCollection<string?> TrailerPlateNumbers,
    string? PhotoReference);

public sealed record NormalizedVehicleEntry(
    string PlateNumber,
    string Brand,
    string Type,
    bool IsServiceVehicle,
    int? PersonsCount,
    IReadOnlyCollection<string> TrailerPlateNumbers,
    string? PhotoReference);

public sealed record VehicleEntryValidationIssue(
    string Field,
    string Code,
    string Message);

public sealed record VehicleEntryValidationResult(
    NormalizedVehicleEntry? Value,
    IReadOnlyCollection<VehicleEntryValidationIssue> Issues)
{
    public bool IsValid => Value is not null && Issues.Count == 0;
}

public sealed class VehicleEntryValidationService
{
    private const int MaximumPlateLength = 16;
    private const int MaximumDescriptorLength = 80;
    private const int MaximumPhotoReferenceLength = 2048;
    private const int MaximumPersonsCount = 100;

    public VehicleEntryValidationResult Validate(VehicleEntryDraft draft)
    {
        var issues = new List<VehicleEntryValidationIssue>();
        var plateNumber = NormalizePlate(draft.PlateNumber);
        var brand = draft.Brand?.Trim() ?? string.Empty;
        var type = draft.Type?.Trim() ?? string.Empty;
        var photoReference = NullIfWhiteSpace(draft.PhotoReference);

        ValidatePlate(plateNumber, "plateNumber", issues);
        ValidateDescriptor(brand, "brand", "Vehicle brand", issues);
        ValidateDescriptor(type, "type", "Vehicle type", issues);

        if (photoReference is { Length: > MaximumPhotoReferenceLength })
        {
            issues.Add(new(
                "photoReference",
                "vehicle_entry.photo_reference_too_long",
                $"Photo reference cannot exceed {MaximumPhotoReferenceLength} characters."));
        }

        var trailerPlates = draft.TrailerPlateNumbers
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(NormalizePlate)
            .ToArray();

        if (trailerPlates.Length > 2)
        {
            issues.Add(new(
                "trailerPlateNumbers",
                "vehicle_entry.too_many_trailers",
                "A vehicle entry can contain at most two trailers."));
        }

        foreach (var trailerPlate in trailerPlates)
        {
            ValidatePlate(trailerPlate, "trailerPlateNumbers", issues);
        }

        if (trailerPlates.Distinct(StringComparer.Ordinal).Count() != trailerPlates.Length ||
            trailerPlates.Contains(plateNumber, StringComparer.Ordinal))
        {
            issues.Add(new(
                "trailerPlateNumbers",
                "vehicle_entry.duplicate_plate",
                "Vehicle and trailer plate numbers must be unique."));
        }

        if (draft.IsServiceVehicle)
        {
            if (draft.PersonsCount is not null)
            {
                issues.Add(new(
                    "personsCount",
                    "vehicle_entry.service_vehicle_persons_not_allowed",
                    "Persons count is not used for a service vehicle."));
            }

            if (trailerPlates.Length != 0)
            {
                issues.Add(new(
                    "trailerPlateNumbers",
                    "vehicle_entry.service_vehicle_trailers_not_allowed",
                    "Trailers are not used for a service vehicle."));
            }
        }
        else if (draft.PersonsCount is null or < 1 or > MaximumPersonsCount)
        {
            issues.Add(new(
                "personsCount",
                "vehicle_entry.invalid_persons_count",
                $"Persons count must be between 1 and {MaximumPersonsCount}."));
        }

        return issues.Count == 0
            ? new VehicleEntryValidationResult(
                new NormalizedVehicleEntry(
                    plateNumber,
                    brand,
                    type,
                    draft.IsServiceVehicle,
                    draft.IsServiceVehicle ? null : draft.PersonsCount,
                    draft.IsServiceVehicle ? [] : trailerPlates,
                    photoReference),
                [])
            : new VehicleEntryValidationResult(null, issues);
    }

    public string NormalizePlate(string? value)
    {
        var normalized = (value ?? string.Empty)
            .Normalize(NormalizationForm.FormKC)
            .ToUpper(CultureInfo.InvariantCulture);
        var result = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (!char.IsWhiteSpace(character) && character is not '-' and not '\u2010' and not '\u2011')
            {
                result.Append(character);
            }
        }

        return result.ToString();
    }

    public bool IsValidPlate(string plateNumber) =>
        plateNumber.Length is >= 2 and <= MaximumPlateLength &&
        plateNumber.All(char.IsLetterOrDigit);

    public bool SupportsEntry(GateMode mode) =>
        mode is GateMode.RearExit or GateMode.BorderEntry;

    public MovementDirection GetDirection(GateMode mode) => mode switch
    {
        GateMode.RearExit => MovementDirection.Exit,
        GateMode.BorderEntry => MovementDirection.Entry,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Gate mode does not receive vehicles.")
    };

    private static void ValidatePlate(
        string value,
        string field,
        ICollection<VehicleEntryValidationIssue> issues)
    {
        if (value.Length is < 2 or > MaximumPlateLength || value.Any(character => !char.IsLetterOrDigit(character)))
        {
            issues.Add(new(
                field,
                "vehicle_entry.invalid_plate",
                $"Plate number must contain 2 to {MaximumPlateLength} letters or digits."));
        }
    }

    private static void ValidateDescriptor(
        string value,
        string field,
        string label,
        ICollection<VehicleEntryValidationIssue> issues)
    {
        if (value.Length is < 1 or > MaximumDescriptorLength)
        {
            issues.Add(new(
                field,
                $"vehicle_entry.invalid_{field}",
                $"{label} must contain 1 to {MaximumDescriptorLength} characters."));
        }
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

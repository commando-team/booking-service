using System.Diagnostics.Metrics;

namespace API.Telemetry;

public static class BookingMetrics
{
    public const string MeterName = "BookingService.API";

    private static readonly Meter Meter = new(MeterName);

    private static readonly Counter<long> Created = Meter.CreateCounter<long>(
        "bookings.created",
        description: "Number of bookings created.");

    private static readonly Counter<long> Confirmed = Meter.CreateCounter<long>(
        "bookings.confirmed",
        description: "Number of bookings confirmed by check-in.");

    private static readonly Counter<long> Cancelled = Meter.CreateCounter<long>(
        "bookings.cancelled",
        description: "Number of bookings cancelled.");

    private static readonly Counter<long> Conflicts = Meter.CreateCounter<long>(
        "bookings.conflicts",
        description: "Number of booking attempts rejected because the time slot is already taken.");

    public static void BookingCreated() => Created.Add(1);

    public static void BookingConfirmed() => Confirmed.Add(1);

    public static void BookingCancelledByUser() =>
        Cancelled.Add(1, new KeyValuePair<string, object?>("reason", "user"));

    public static void BookingConflict() => Conflicts.Add(1);
}

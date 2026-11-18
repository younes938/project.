using ETicket.Domain.Common;
using ETicket.Domain.Models;

namespace ETicket.Domain.Tests;

public sealed class StatisticsFilterTests
{
    [Fact]
    public void Constructor_AllowsPeriodOfExactlyOneHundredEightyDays()
    {
        var from = new DateOnly(2026, 1, 1);

        var filter = new StatisticsFilter(
            from,
            from.AddDays(180),
            null,
            null,
            null);

        Assert.Equal(180, filter.To.DayNumber - filter.From.DayNumber);
    }

    [Fact]
    public void Constructor_RejectsPeriodLongerThanOneHundredEightyDays()
    {
        var from = new DateOnly(2026, 1, 1);

        var exception = Assert.Throws<DomainRuleViolationException>(
            () => new StatisticsFilter(
                from,
                from.AddDays(181),
                null,
                null,
                null));

        Assert.Equal("statistics.period_too_long", exception.Code);
    }
}

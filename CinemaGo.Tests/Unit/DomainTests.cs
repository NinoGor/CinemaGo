using CinemaGo.Domain.Exceptions;
using CinemaGo.Domain.Models.Entities.Enums;
using CinemaGo.Tests.Helpers;

namespace CinemaGo.Tests.Unit;

public class DomainTests
{
	// პარამეტრული ტესტი
    [Theory]
    [InlineData(false, 10)]
    [InlineData(true, 25)]
    public void GetPriceFor_ReturnsVipOrStandardPrice(bool isVip, int expected)
    {
        var session = TestData.Session(standard: 10m, vip: 25m);

        Assert.Equal(expected, session.GetPriceFor(TestData.Seat(1, isVip: isVip)));
    }

    [Fact]
    public void OverlapsWith_DetectsIntersectionButNotTouchingRanges()
    {
        var session = TestData.Session(startsAt: TestData.Now.AddHours(10)); // 10:00 -> 12:00 (120 min)

        Assert.True(session.OverlapsWith(session.StartsAt.AddMinutes(30), session.EndsAt.AddMinutes(30)));
        Assert.True(session.OverlapsWith(session.StartsAt.AddMinutes(-30), session.StartsAt.AddMinutes(30)));
        Assert.False(session.OverlapsWith(session.EndsAt, session.EndsAt.AddHours(2)));          // starts exactly when it ends
        Assert.False(session.OverlapsWith(session.StartsAt.AddHours(-2), session.StartsAt));     // ends exactly when it starts
    }

    [Fact]
    public void Booking_Cancel_Twice_ThrowsConflict()
    {
        var booking = TestData.Booking(TestData.Session(), TestData.Seat(1));

        booking.Cancel(TestData.Now);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Throws<ConflictException>(() => booking.Cancel(TestData.Now));
    }
}

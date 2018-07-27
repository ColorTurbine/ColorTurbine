using System;
using ColorTurbine;
using Xunit;

namespace ColorTurbine.tests
{
    public class TimeTests
    {
        // [Fact]
        // public void Sunrise_today_should_be_correct()
        // {
        //     var time = new DateTime(2018, 07, 08, 00, 00, 00, DateTimeKind.Utc) + TimeSpan.FromHours(5);
        //     time = TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);

        //     var calculated = TimeHelpers.TimeToNextSunChange(time, 44.9480, -93.2936);

        //     var sunrise = new DateTime(2018, 07, 08, 05, 35, 01, 875, DateTimeKind.Utc) + TimeSpan.FromHours(5);
        //     sunrise = TimeZoneInfo.ConvertTimeFromUtc(sunrise, TimeZoneInfo.Local);
        //     Assert.Equal(sunrise - time, calculated);
        // }

        // [Fact]
        // public void Sunset_should_be_correct()
        // {
        //     var time = new DateTime(2018, 07, 08, 12, 00, 00, DateTimeKind.Utc) + TimeSpan.FromHours(5); // Noon, but can be anytime after sunrise
        //     time = TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);
            
        //     var calculated = TimeHelpers.TimeToNextSunChange(time, 44.9480, -93.2936);

        //     var sunset = new DateTime(2018, 07, 08, 21, 01, 24, 741, DateTimeKind.Utc) + TimeSpan.FromHours(5);
        //     sunset = TimeZoneInfo.ConvertTimeFromUtc(sunset, TimeZoneInfo.Local);
        //     Assert.Equal(sunset - time, calculated);
        // }

        // [Fact]
        // public void Sunrise_tomorrow_should_be_correct()
        // {
        //     var time = new DateTime(2018, 07, 08, 23, 00, 00, DateTimeKind.Utc) + TimeSpan.FromHours(5); // 11 PM, but can be anytime after sunset
        //     time = TimeZoneInfo.ConvertTimeFromUtc(time, TimeZoneInfo.Local);
            
        //     var calculated = TimeHelpers.TimeToNextSunChange(time, 44.9480, -93.2936);

        //     var sunrise = new DateTime(2018, 07, 09, 05, 35, 47, 510, DateTimeKind.Utc) + TimeSpan.FromHours(5);
        //     sunrise = TimeZoneInfo.ConvertTimeFromUtc(sunrise, TimeZoneInfo.Local);
        //     Assert.Equal(sunrise - time, calculated);
        // }
    }
}

namespace Winzer.Impl.Util;
public static class DateTimeExtensions
{
    public const string DefaultTimeZone = "America/New_York";
    public static TimeZoneInfo DefaultTimeZoneInfo {
        get {
            return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZone);
        }
    }
    public static DateTime ConvertToDefaultTimeZone(this DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utc, DefaultTimeZoneInfo);
    }

    public const string PacificTimeZone = "America/Los_Angeles";
    public static TimeZoneInfo PacificTimeZoneInfo {
        get {
            return TimeZoneInfo.FindSystemTimeZoneById(PacificTimeZone);
        }
    }
    public static DateTime ConvertToPacificTimeZone(this DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utc, PacificTimeZoneInfo);
    }
}

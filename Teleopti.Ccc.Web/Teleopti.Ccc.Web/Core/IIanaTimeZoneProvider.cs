namespace Teleopti.Ccc.Web.Core
{
    public interface IIanaTimeZoneProvider
    {
        string IanaToWindows(string ianaZoneId);
        string WindowsToIana(string windowsZoneId);
    }
}
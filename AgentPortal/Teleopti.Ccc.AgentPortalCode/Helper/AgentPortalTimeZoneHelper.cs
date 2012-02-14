using System;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    /// NOTE : TimeZone class always uses local time zone to calculte Utc time.  TimeZoneInfo class can not use since AP is running on Framework 2.0
    
    /// <summary>
    /// Represents a calss that provided TimeZone convertion based on Logged Persons
    /// Default TimeZones UTC offset.
    /// </summary>
    public static class AgentPortalTimeZoneHelper
    {
        public static DateTime ConvertToUniversalTime(DateTime dateTime)
        {
            DateTime newDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            DateTime returnDateTime;
            bool returnBool;
            SdkServiceHelper.TeleoptiSdkService.ConvertToUtc(newDateTime, true, StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.TimeZoneId, out returnDateTime, out returnBool);
            return returnDateTime;
        }

        public static DateTime ConvertFromUniversalTime(DateTime dateTime)
        {
            DateTime newDateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            DateTime returnDateTime;
            bool returnBool;
            SdkServiceHelper.TeleoptiSdkService.ConvertFromUtc(newDateTime, true, StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson.TimeZoneId, out returnDateTime, out returnBool);
            return returnDateTime;
        }
    }
}

using System;
using System.Diagnostics;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    /// <summary>
    /// This class contains helper functions 
    /// </summary>
    /// <remarks>
    /// Created by: MuhamadR
    /// Created date: 2008-02-29
    /// </remarks>
    public static class HelperFunctions
    {
        /// <summary>
        /// Determines whether [is design mode].
        /// The DesignMode property does not work in constructors so use this one instead.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is design mode]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-10-12
        /// </remarks>
        public static bool RuntimeMode()
        {
            return (Process.GetCurrentProcess().ProcessName.IndexOf("devenv", StringComparison.CurrentCultureIgnoreCase) == -1);
        }

        public static DateTimePeriodDto CreateDateTimePeriodDto(DateTime localStartDateTime, DateTime localEndDateTime)
        {
            DateTimePeriodDto period = new DateTimePeriodDto();
            period.UtcStartTime = AgentPortalTimeZoneHelper.ConvertToUniversalTime(localStartDateTime);
            period.UtcEndTime = AgentPortalTimeZoneHelper.ConvertToUniversalTime(localEndDateTime);
            period.LocalStartDateTime = localStartDateTime;
            period.LocalEndDateTime = localEndDateTime;
            period.UtcStartTimeSpecified = true;
            period.UtcEndTimeSpecified = true;
            return period;
        }
    }
}
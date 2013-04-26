using System;
using System.Diagnostics;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

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
            var timeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    StateHolder.Instance.State.SessionScopeData.LoggedOnPerson.TimeZoneId);
            period.UtcStartTime = TimeZoneHelper.ConvertToUtc(localStartDateTime, timeZone);
            period.UtcEndTime = TimeZoneHelper.ConvertToUtc(localEndDateTime, timeZone);
            period.LocalStartDateTime = localStartDateTime;
            period.LocalEndDateTime = localEndDateTime;
            return period;
        }
    }
}
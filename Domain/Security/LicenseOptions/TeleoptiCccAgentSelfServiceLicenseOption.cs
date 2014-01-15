using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
    /// <summary>
    /// Represents the Raptor Ass license option
    /// </summary>
    public class TeleoptiCccAgentSelfServiceLicenseOption : LicenseOption
    {

        #region Interface

        /// <summary>
        /// Initializes a new instance of the <see cref="TeleoptiCccAgentSelfServiceLicenseOption"/> class.
        /// </summary>
        public TeleoptiCccAgentSelfServiceLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccAgentSelfService, DefinedLicenseOptionNames.TeleoptiCccAss)
        {
            //
        }

        /// <summary>
        /// Sets all application functions.
        /// </summary>
        /// <param name="allApplicationFunctions"></param>
        /// <value>The enabled application functions.</value>
        /// <remarks>
        /// CCC Agent selfservice option is an aggregate license option to the following options:
        /// <list type="bullet">
        /// 	<item>
        /// 		<description>ShiftTrades</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>AgentScheduleMessenger</description>
        /// 	</item>
        /// 	<item>
        /// 		<description>HolidayPlanner</description>
        /// 	</item>
        /// </list>
        /// </remarks>
        public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
        {
            EnabledApplicationFunctions.Clear();
            TeleoptiCccShiftTradesLicenseOption shiftTradesLicenseOption = new TeleoptiCccShiftTradesLicenseOption();
            shiftTradesLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
            foreach (IApplicationFunction applicationFunction in shiftTradesLicenseOption.EnabledApplicationFunctions)
                EnabledApplicationFunctions.Add(applicationFunction);

            TeleoptiCccAgentScheduleMessengerLicenseOption scheduleMessengerLicenseOption = new TeleoptiCccAgentScheduleMessengerLicenseOption();
            scheduleMessengerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
            foreach (IApplicationFunction applicationFunction in scheduleMessengerLicenseOption.EnabledApplicationFunctions)
                EnabledApplicationFunctions.Add(applicationFunction);

            TeleoptiCccHolidayPlannerLicenseOption holidayPlannerLicenseOption = new TeleoptiCccHolidayPlannerLicenseOption();
            holidayPlannerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
            foreach (IApplicationFunction applicationFunction in holidayPlannerLicenseOption.EnabledApplicationFunctions)
                EnabledApplicationFunctions.Add(applicationFunction);
        }

        #endregion
    }
}
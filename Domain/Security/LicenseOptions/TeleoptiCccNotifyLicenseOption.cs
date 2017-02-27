using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccNotifyLicenseOption : LicenseOption
	{
		public TeleoptiCccNotifyLicenseOption()
			: base(DefinedLicenseOptionPaths.TeleoptiCccNotify, DefinedLicenseOptionNames.TeleoptiCccNotify)
		{
		}

		public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
		{
			var realTimeAdherenceLicenseOption = new TeleoptiCccRealTimeAdherenceLicenseOption();
			realTimeAdherenceLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			IEnumerable<IApplicationFunction> functions = realTimeAdherenceLicenseOption.EnabledApplicationFunctions;

			var scheduleMessengerLicenseOption = new TeleoptiCccAgentScheduleMessengerLicenseOption();
			scheduleMessengerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions = functions.Union(scheduleMessengerLicenseOption.EnabledApplicationFunctions);

			var smsLinkLicenseOption = new TeleoptiCccSmsLinkLicenseOption();
			smsLinkLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions = functions.Union(smsLinkLicenseOption.EnabledApplicationFunctions);

			var calendarLinkLicenseOption = new TeleoptiCccCalendarLinkLicenseOption();
			calendarLinkLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions = functions.Union(calendarLinkLicenseOption.EnabledApplicationFunctions);

			EnableFunctions(functions.ToArray());
		}
	}
}
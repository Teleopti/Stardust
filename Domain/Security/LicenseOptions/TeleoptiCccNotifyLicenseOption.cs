using System.Collections.Generic;
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
			var functions = new List<IApplicationFunction>();
			var realTimeAdherenceLicenseOption = new TeleoptiCccRealTimeAdherenceLicenseOption();
			realTimeAdherenceLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions.AddRange(realTimeAdherenceLicenseOption.EnabledApplicationFunctions);

			var scheduleMessengerLicenseOption = new TeleoptiCccAgentScheduleMessengerLicenseOption();
			scheduleMessengerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions.AddRange(scheduleMessengerLicenseOption.EnabledApplicationFunctions);

			var smsLinkLicenseOption = new TeleoptiCccSmsLinkLicenseOption();
			smsLinkLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions.AddRange(smsLinkLicenseOption.EnabledApplicationFunctions);

			var calendarLinkLicenseOption = new TeleoptiCccCalendarLinkLicenseOption();
			calendarLinkLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions.AddRange(calendarLinkLicenseOption.EnabledApplicationFunctions);

			EnableFunctions(functions.ToArray());
		}
	}
}
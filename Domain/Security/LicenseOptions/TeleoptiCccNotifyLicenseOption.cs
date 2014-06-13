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

		public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
		{
			EnabledApplicationFunctions.Clear();

			var realTimeAdherenceLicenseOption = new TeleoptiCccRealTimeAdherenceLicenseOption();
			realTimeAdherenceLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			foreach (IApplicationFunction applicationFunction in realTimeAdherenceLicenseOption.EnabledApplicationFunctions)
				EnabledApplicationFunctions.Add(applicationFunction);

			var scheduleMessengerLicenseOption = new TeleoptiCccAgentScheduleMessengerLicenseOption();
			scheduleMessengerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			foreach (IApplicationFunction applicationFunction in scheduleMessengerLicenseOption.EnabledApplicationFunctions)
				EnabledApplicationFunctions.Add(applicationFunction);

			var smsLinkLicenseOption = new TeleoptiCccSmsLinkLicenseOption();
			smsLinkLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			foreach (IApplicationFunction applicationFunction in smsLinkLicenseOption.EnabledApplicationFunctions)
				EnabledApplicationFunctions.Add(applicationFunction);

			var calendarLinkLicenseOption = new TeleoptiCccCalendarLinkLicenseOption();
			calendarLinkLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			foreach (IApplicationFunction applicationFunction in calendarLinkLicenseOption.EnabledApplicationFunctions)
				EnabledApplicationFunctions.Add(applicationFunction);

			
		}
	}
}
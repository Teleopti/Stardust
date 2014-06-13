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

			var scheduleMessengerLicenseOption = new TeleoptiCccAgentScheduleMessengerLicenseOption();
			scheduleMessengerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			foreach (IApplicationFunction applicationFunction in scheduleMessengerLicenseOption.EnabledApplicationFunctions)
				EnabledApplicationFunctions.Add(applicationFunction);
		}
	}
}
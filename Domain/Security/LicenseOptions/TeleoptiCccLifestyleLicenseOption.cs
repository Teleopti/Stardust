using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccLifestyleLicenseOption : LicenseOption
    {
        public TeleoptiCccLifestyleLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccLifestyle, DefinedLicenseOptionNames.TeleoptiCccAss)
        {
        }

        public override void EnableApplicationFunctions(IList<IApplicationFunction> allApplicationFunctions)
        {
            EnabledApplicationFunctions.Clear();

            var shiftTradesLicenseOption = new TeleoptiCccShiftTraderLicenseOption();
            shiftTradesLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
            foreach (IApplicationFunction applicationFunction in shiftTradesLicenseOption.EnabledApplicationFunctions)
                EnabledApplicationFunctions.Add(applicationFunction);

            var vacationPlannerLicenseOption = new TeleoptiCccVacationPlannerLicenseOption();
            vacationPlannerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
            foreach (IApplicationFunction applicationFunction in vacationPlannerLicenseOption.EnabledApplicationFunctions)
                EnabledApplicationFunctions.Add(applicationFunction);

			var overtimeAvailabilityLicenseOption = new TeleoptiCccOvertimeAvailabilityLicenseOption();
			overtimeAvailabilityLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			foreach (IApplicationFunction applicationFunction in overtimeAvailabilityLicenseOption.EnabledApplicationFunctions)
				EnabledApplicationFunctions.Add(applicationFunction);
        }
    }
}
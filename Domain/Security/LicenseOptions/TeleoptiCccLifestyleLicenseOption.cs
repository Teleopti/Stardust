using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.LicenseOptions
{
	public class TeleoptiCccLifestyleLicenseOption : LicenseOption
    {
        public TeleoptiCccLifestyleLicenseOption()
            : base(DefinedLicenseOptionPaths.TeleoptiCccLifestyle, DefinedLicenseOptionNames.TeleoptiCccLifestyle)
        {
        }

        public override void EnableApplicationFunctions(IEnumerable<IApplicationFunction> allApplicationFunctions)
        {
	        var functions = new List<IApplicationFunction>();
            var shiftTradesLicenseOption = new TeleoptiCccShiftTraderLicenseOption();
            shiftTradesLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions.AddRange(shiftTradesLicenseOption.EnabledApplicationFunctions);
			
            var vacationPlannerLicenseOption = new TeleoptiCccVacationPlannerLicenseOption();
            vacationPlannerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions.AddRange(vacationPlannerLicenseOption.EnabledApplicationFunctions);
                
			var overtimeAvailabilityLicenseOption = new TeleoptiCccOvertimeAvailabilityLicenseOption();
			overtimeAvailabilityLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
			functions.AddRange(overtimeAvailabilityLicenseOption.EnabledApplicationFunctions);
			
			EnableFunctions(functions.ToArray());
        }
    }
}
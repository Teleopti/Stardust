using System.Collections.Generic;
using System.Linq;
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
            var shiftTradesLicenseOption = new TeleoptiCccShiftTraderLicenseOption();
            shiftTradesLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
	        IEnumerable<IApplicationFunction> functions = shiftTradesLicenseOption.EnabledApplicationFunctions;
			
            var vacationPlannerLicenseOption = new TeleoptiCccVacationPlannerLicenseOption();
            vacationPlannerLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
	        functions = functions.Union(vacationPlannerLicenseOption.EnabledApplicationFunctions);
                
			var overtimeAvailabilityLicenseOption = new TeleoptiCccOvertimeAvailabilityLicenseOption();
			overtimeAvailabilityLicenseOption.EnableApplicationFunctions(allApplicationFunctions);
	        functions = functions.Union(overtimeAvailabilityLicenseOption.EnabledApplicationFunctions);
			
			EnableFunctions(functions.ToArray());
        }
    }
}
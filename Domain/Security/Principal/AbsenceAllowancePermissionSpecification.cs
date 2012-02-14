using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.LicenseOptions;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public class AbsenceAllowancePermissionSpecification : Specification<IEnumerable<ClaimSet>>
    {
        public override bool IsSatisfiedBy(IEnumerable<ClaimSet> obj)
        {
            var holidayPlannerLicenseOption = new TeleoptiCccHolidayPlannerLicenseOption();
            holidayPlannerLicenseOption.EnableApplicationFunctions(
                new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList.ToList());

            return holidayPlannerLicenseOption.EnabledApplicationFunctions.All(a => obj.Any(o => o.FindClaims(
                string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/",
                              a.FunctionPath), Rights.PossessProperty).Any()));
        }
    }
}
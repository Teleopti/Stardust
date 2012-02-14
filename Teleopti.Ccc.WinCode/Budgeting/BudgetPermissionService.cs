using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting
{
    public class BudgetPermissionService : IBudgetPermissionService
    {
        private bool? _isAllowancePermitted;

        public bool IsAllowancePermitted
        {
            get
           {
               if(!_isAllowancePermitted.HasValue)
                   _isAllowancePermitted = TeleoptiPrincipal.Current.PrincipalAuthorization.EvaluateSpecification(new AbsenceAllowancePermissionSpecification());
               return _isAllowancePermitted.GetValueOrDefault(false);
           }
        }
    }
}

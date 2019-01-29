using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
    public class BudgetPermissionService : IBudgetPermissionService
    {
        private bool? _isAllowancePermitted;

        public bool IsAllowancePermitted
        {
            get
           {
               if (!_isAllowancePermitted.HasValue)
                   _isAllowancePermitted =
                       PrincipalAuthorization.Current_DONTUSE().IsPermitted(
                           DefinedRaptorApplicationFunctionPaths.RequestAllowances);
               return _isAllowancePermitted.GetValueOrDefault(false);
           }
        }
    }
}

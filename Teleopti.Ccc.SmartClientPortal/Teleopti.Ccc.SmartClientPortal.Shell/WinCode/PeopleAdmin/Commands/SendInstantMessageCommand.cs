using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface ISendInstantMessageCommand : ICanExecute  //, IExecutableCommand
    { }

    public class SendInstantMessageCommand :ISendInstantMessageCommand
    {
        public bool CanExecute()
        {
            return PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.SendAsm);
        }
    }
}
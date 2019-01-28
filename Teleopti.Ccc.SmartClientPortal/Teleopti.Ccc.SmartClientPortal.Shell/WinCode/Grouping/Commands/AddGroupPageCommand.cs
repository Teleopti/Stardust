using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands
{
    public interface IAddGroupPageCommand : IExecutableCommand, ICanExecute
    {

    }
    public class AddGroupPageCommand : IAddGroupPageCommand
    {
        private readonly IPersonSelectorView _personSelectorView;

        public AddGroupPageCommand(IPersonSelectorView personSelectorView)
        {
            _personSelectorView = personSelectorView;
        }

        public void Execute()
        {
            if(CanExecute())
                _personSelectorView.AddNewGroupPage();
        }

        public bool CanExecute()
        {
            return
                PrincipalAuthorization.Current_DONTUSE().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.ModifyGroupPage);
        }
    }
}
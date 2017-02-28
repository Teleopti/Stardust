using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Grouping.Commands
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
                PrincipalAuthorization.Current().IsPermitted(
                    DefinedRaptorApplicationFunctionPaths.ModifyGroupPage);
        }
    }
}
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Grouping.Commands
{
    public interface IRenameGroupPageCommand : IExecutableCommand, ICanExecute {}

    public class RenameGroupPageCommand : IRenameGroupPageCommand
    {
        private readonly IPersonSelectorView _personSelectorView;

        public RenameGroupPageCommand(IPersonSelectorView personSelectorView)
        {
            _personSelectorView = personSelectorView;
        }

        public void Execute()
        {
            if (CanExecute())
            {
                var tab = _personSelectorView.SelectedTab;
            var command = tab.Tag as ILoadUserDefinedTabsCommand;
            if(command != null)
                _personSelectorView.RenameGroupPage(command.Id, tab.Text);
            }
        }

        public bool CanExecute()
        {
            if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyGroupPage))
                return false;

            return _personSelectorView.SelectedTab.Tag as ILoadUserDefinedTabsCommand != null;
        }
    }
}
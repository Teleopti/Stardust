using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands
{
    public interface IModifyGroupPageCommand : IExecutableCommand, ICanExecute {}

    public class ModifyGroupPageCommand : IModifyGroupPageCommand
    {
        private readonly IPersonSelectorView _personSelectorView;

        public ModifyGroupPageCommand(IPersonSelectorView personSelectorView)
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
                _personSelectorView.ModifyGroupPage(command.Id);
            }
        }

        public bool CanExecute()
        {
            if (!PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPeopleWithinGroupPage))
                return false;

            return _personSelectorView.SelectedTab.Tag as ILoadUserDefinedTabsCommand != null;
        }
    }
}
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Commands;
using Teleopti.Ccc.WinCode.Grouping.Events;
using Teleopti.Ccc.WinCode.PeopleAdmin.Commands;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public interface IPeopleNavigatorPresenter
    {
        void Init(IPeopleNavigator navigator);
    }

    public class PeopleNavigatorPresenter : IPeopleNavigatorPresenter
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IOpenModuleCommand _openModuleCommand;
        private readonly IAddGroupPageCommand _addGroupPageCommand;
        private readonly IDeleteGroupPageCommand _deleteGroupPageCommand;
        private readonly IModifyGroupPageCommand _modifyGroupPageCommand;
        private readonly IRenameGroupPageCommand _renameGroupPageCommand;
        private readonly ISendInstantMessageCommand _sendInstantMessageCommand;
    	private readonly ISendInstantMessageEnableCommand _sendInstantMessageEnableCommand;
        private readonly IAddPersonEnableCommand _addPersonEnableCommand;
        private IPeopleNavigator _navigator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public PeopleNavigatorPresenter(IEventAggregator eventAggregator, IOpenModuleCommand openModuleCommand, IAddGroupPageCommand addGroupPageCommand, 
            IDeleteGroupPageCommand deleteGroupPageCommand, IModifyGroupPageCommand modifyGroupPageCommand, 
            IRenameGroupPageCommand renameGroupPageCommand, ISendInstantMessageCommand sendInstantMessageCommand, 
            ISendInstantMessageEnableCommand sendInstantMessageEnableCommand,IAddPersonEnableCommand addPersonEnableCommand)
        {
            _eventAggregator = eventAggregator;
            _openModuleCommand = openModuleCommand;
            _addGroupPageCommand = addGroupPageCommand;
            _deleteGroupPageCommand = deleteGroupPageCommand;
            _modifyGroupPageCommand = modifyGroupPageCommand;
            _renameGroupPageCommand = renameGroupPageCommand;
            _sendInstantMessageCommand = sendInstantMessageCommand;
        	_sendInstantMessageEnableCommand = sendInstantMessageEnableCommand;
            _addPersonEnableCommand = addPersonEnableCommand;
            _eventAggregator.GetEvent<SelectedNodesChanged>().Subscribe(selectedNodesChanged);
            _eventAggregator.GetEvent<OpenModuleClicked>().Subscribe(openModuleClicked);
            _eventAggregator.GetEvent<FindPeopleClicked>().Subscribe(findPeopleClicked);
            
        }

        private void findPeopleClicked(string obj)
        {
            _navigator.FindPeople();
        }

        private void openModuleClicked(string obj)
        {
            _navigator.Open();
        }

        // needed because how PeopleNavigator is used via autofac
        public void Init(IPeopleNavigator navigator)
        {
            _navigator = navigator;
        }

        private void selectedNodesChanged(string obj)
        {
            updateView();
        }

        private void updateView()
        {
            _navigator.OpenEnabled = _openModuleCommand.CanExecute();
            _navigator.AddGroupPageEnabled = _addGroupPageCommand.CanExecute();
            _navigator.DeleteGroupPageEnabled = _deleteGroupPageCommand.CanExecute();
            _navigator.ModifyGroupPageEnabled = _modifyGroupPageCommand.CanExecute();
            _navigator.RenameGroupPageEnabled = _renameGroupPageCommand.CanExecute();
            _navigator.SendMessageVisible = _sendInstantMessageCommand.CanExecute();
            _navigator.SendMessageEnable = _sendInstantMessageEnableCommand.CanExecute();
            _navigator.AddNewEnabled = _addPersonEnableCommand.CanExecute();
        }
    }
}
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
    public interface IScheduleNavigatorPresenter
    {
        void Init(IScheduleNavigator navigator);
        IScheduleNavigator Navigator { get; }
    }

    public class ScheduleNavigatorPresenter : IScheduleNavigatorPresenter
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IOpenModuleCommand _openModuleCommand;
        private readonly IAddGroupPageCommand _addGroupPageCommand;
        private readonly IDeleteGroupPageCommand _deleteGroupPageCommand;
        private readonly IModifyGroupPageCommand _modifyGroupPageCommand;
        private readonly IRenameGroupPageCommand _renameGroupPageCommand;
        private readonly IOpenMeetingsOverviewCommand _openMeetingsOverviewCommand;
        private readonly IAddMeetingFromPanelCommand _addMeetingFromPanelCommand;
        private readonly IOpenIntradayTodayCommand _openIntradayTodayCommand;

        private IScheduleNavigator _navigator;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ScheduleNavigatorPresenter(IEventAggregator eventAggregator, IOpenModuleCommand openModuleCommand, 
            IAddGroupPageCommand addGroupPageCommand, IDeleteGroupPageCommand deleteGroupPageCommand, 
            IModifyGroupPageCommand modifyGroupPageCommand, IRenameGroupPageCommand renameGroupPageCommand, 
            IOpenMeetingsOverviewCommand openMeetingsOverviewCommand,IAddMeetingFromPanelCommand addMeetingFromPanelCommand,
            IOpenIntradayTodayCommand openIntradayTodayCommand)
        {
            _eventAggregator = eventAggregator;
            _openModuleCommand = openModuleCommand;
            _addGroupPageCommand = addGroupPageCommand;
            _deleteGroupPageCommand = deleteGroupPageCommand;
            _modifyGroupPageCommand = modifyGroupPageCommand;
            _renameGroupPageCommand = renameGroupPageCommand;
            _openMeetingsOverviewCommand = openMeetingsOverviewCommand;
            _addMeetingFromPanelCommand = addMeetingFromPanelCommand;
            _openIntradayTodayCommand = openIntradayTodayCommand;

            _eventAggregator.GetEvent<SelectedNodesChanged>().Subscribe(selectedNodesChanged);
            _eventAggregator.GetEvent<OpenModuleClicked>().Subscribe(openModuleClicked);
            _eventAggregator.GetEvent<OpenMeetingsOverviewClicked>().Subscribe(openMeetingsOverView);
            _eventAggregator.GetEvent<AddMeetingFromPanelClicked>().Subscribe(addMeeting);
        }

        private void addMeeting(string obj)
        {
            _addMeetingFromPanelCommand.Execute();
        }

        private void openMeetingsOverView(string obj)
        {
            _openMeetingsOverviewCommand.Execute();
        }

        private void openModuleClicked(string obj)
        {
            _navigator.Open();
        }

        // needed because how PeopleNavigator is used via autofac
        public void Init(IScheduleNavigator navigator)
        {
            _navigator = navigator;
        }

        public IScheduleNavigator Navigator
        {
            get { return _navigator; }
        }

        private void selectedNodesChanged(string obj)
        {
            updateView();
        }

        private void updateView()
        {
            _navigator.OpenEnabled = _openModuleCommand.CanExecute();
            _navigator.OpenIntradayTodayEnabled = _openIntradayTodayCommand.CanExecute();
            _navigator.AddGroupPageEnabled = _addGroupPageCommand.CanExecute();
            _navigator.DeleteGroupPageEnabled = _deleteGroupPageCommand.CanExecute();
            _navigator.ModifyGroupPageEnabled = _modifyGroupPageCommand.CanExecute();
            _navigator.RenameGroupPageEnabled = _renameGroupPageCommand.CanExecute();

            _navigator.OpenMeetingOverviewEnabled = _openMeetingsOverviewCommand.CanExecute();
            _navigator.AddMeetingOverviewEnabled = _addMeetingFromPanelCommand.CanExecute();
        }
    }
}
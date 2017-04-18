using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    public class ScheduleNavigatorPresenterTest
    {
        private MockRepository _mocks;
        private IOpenModuleCommand _openModuleCommand;
        private EventAggregator _eventAggregator;
        private ScheduleNavigatorPresenter _target;
        private IScheduleNavigator _view;
        private IAddGroupPageCommand _addGroupPageCommand;
        private IDeleteGroupPageCommand _deleteGroupPageCommand;
        private IModifyGroupPageCommand _modifyGroupPageCommand;
        private IRenameGroupPageCommand _renameGroupPageCommand;
        private IOpenMeetingsOverviewCommand _openMeetingOverviewCommand;
        private IAddMeetingFromPanelCommand _addMeetingFromPanelCommand;
        private IOpenIntradayTodayCommand _openIntradayTodayCommand;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _openModuleCommand = _mocks.StrictMock<IOpenModuleCommand>();
            _addGroupPageCommand = _mocks.StrictMock<IAddGroupPageCommand>();
            _deleteGroupPageCommand = _mocks.StrictMock<IDeleteGroupPageCommand>();
            _modifyGroupPageCommand = _mocks.StrictMock<IModifyGroupPageCommand>();
            _renameGroupPageCommand = _mocks.StrictMock<IRenameGroupPageCommand>();
            _openMeetingOverviewCommand = _mocks.StrictMock<IOpenMeetingsOverviewCommand>();
            _addMeetingFromPanelCommand = _mocks.StrictMock<IAddMeetingFromPanelCommand>();
            _openIntradayTodayCommand = _mocks.StrictMock<IOpenIntradayTodayCommand>();
            _eventAggregator = new EventAggregator();
            _view = _mocks.StrictMock<IScheduleNavigator>();
            _target = new ScheduleNavigatorPresenter(_eventAggregator, _openModuleCommand, _addGroupPageCommand, _deleteGroupPageCommand,
                _modifyGroupPageCommand, _renameGroupPageCommand, _openMeetingOverviewCommand, _addMeetingFromPanelCommand, _openIntradayTodayCommand);
            _target.Init(_view);
        }

        [Test]
        public void ShouldUpdateViewHowCommandCanExecute()
        {
            Expect.Call(_openModuleCommand.CanExecute()).Return(false);
            Expect.Call(_openIntradayTodayCommand.CanExecute()).Return(false);
            Expect.Call(_addGroupPageCommand.CanExecute()).Return(false);
            Expect.Call(_deleteGroupPageCommand.CanExecute()).Return(false);
            Expect.Call(_modifyGroupPageCommand.CanExecute()).Return(false);
            Expect.Call(_renameGroupPageCommand.CanExecute()).Return(false);
            Expect.Call(_openMeetingOverviewCommand.CanExecute()).Return(false);
            Expect.Call(_addMeetingFromPanelCommand.CanExecute()).Return(false);
           
            Expect.Call(() => _view.OpenEnabled = false);
            Expect.Call(() => _view.OpenIntradayTodayEnabled = false);
            Expect.Call(() => _view.AddGroupPageEnabled = false);
            Expect.Call(() => _view.DeleteGroupPageEnabled = false);
            Expect.Call(() => _view.ModifyGroupPageEnabled = false);
            Expect.Call(() => _view.RenameGroupPageEnabled = false);
            Expect.Call(() => _view.OpenMeetingOverviewEnabled = false);
            Expect.Call(() => _view.AddMeetingOverviewEnabled = false);

            _mocks.ReplayAll();
            _eventAggregator.GetEvent<SelectedNodesChanged>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallCommandOnAddMeeting()
        {
            Expect.Call(_addMeetingFromPanelCommand.Execute);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<AddMeetingFromPanelClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallCommandOnOpenOverview()
        {
            Expect.Call(_openMeetingOverviewCommand.Execute);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<OpenMeetingsOverviewClicked>().Publish("");
            _mocks.VerifyAll();
        }
        
        [Test]
        public void ShouldHaveAReferenceToTheView()
        {
            Assert.That(_target.Navigator, Is.EqualTo(_view));
        }

        [Test]
        public void ShouldCallViewOnOpenModule()
        {
            Expect.Call(_view.Open);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<OpenModuleClicked>().Publish("");
            _mocks.VerifyAll();
        }
    }
}
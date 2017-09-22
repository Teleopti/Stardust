using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
    [TestFixture]
    public class MeetingOverviewPresenterTest
    {
        private MockRepository _mocks;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private MeetingOverviewPresenter _target;
        private IMeetingOverviewView _view;
        private IEventAggregator _eventAggregator;
        private IDeleteMeetingCommand _deleteMeetingCommand;
        private IAddMeetingCommand _addMeetingCommand;
        private IEditMeetingCommand _editMeetingCommand;
        private ICopyMeetingCommand _copyMeetingCommand;
        private IPasteMeetingCommand _pasteMeetingCommand;
        private ICutMeetingCommand _cutMeetingCommand;
        private IInfoWindowTextFormatter _infoTextFormatter;
        private IShowExportMeetingCommand _showExportMeetingCommand;
    	private IFetchMeetingForCurrentUserChangeCommand _fetchMeetingForCurrentUser;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMeetingOverviewView>();
            _eventAggregator = new EventAggregator();
            _infoTextFormatter = _mocks.StrictMock<IInfoWindowTextFormatter>();
            _deleteMeetingCommand = _mocks.StrictMock<IDeleteMeetingCommand>();
            _addMeetingCommand = _mocks.StrictMock<IAddMeetingCommand>();
            _editMeetingCommand = _mocks.StrictMock<IEditMeetingCommand>();
            _copyMeetingCommand = _mocks.StrictMock<ICopyMeetingCommand>();
            _pasteMeetingCommand = _mocks.StrictMock<IPasteMeetingCommand>();
            _cutMeetingCommand = _mocks.StrictMock<ICutMeetingCommand>();
            _showExportMeetingCommand = _mocks.StrictMock<IShowExportMeetingCommand>();
        	_fetchMeetingForCurrentUser = _mocks.StrictMock<IFetchMeetingForCurrentUserChangeCommand>();
            _target = new MeetingOverviewPresenter(_view, _eventAggregator, _infoTextFormatter, _deleteMeetingCommand, _addMeetingCommand,
				_editMeetingCommand, _cutMeetingCommand, _copyMeetingCommand, _pasteMeetingCommand, _showExportMeetingCommand, _fetchMeetingForCurrentUser);
        }

        [Test]
        public void ShouldCallDeleteCommandOnDelete()
        {
            Expect.Call(() => _deleteMeetingCommand.Execute());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<DeleteAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallAddCommandOnAdd()
        {
            Expect.Call(() => _addMeetingCommand.Execute());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<AddAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallEditCommandOnEdit()
        {
            Expect.Call(() => _editMeetingCommand.Execute());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<EditAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallCutCommandOnCut()
        {
            Expect.Call(() => _cutMeetingCommand.Execute());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<CutAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallCopyCommandOnCopy()
        {
            Expect.Call(() => _copyMeetingCommand.Execute());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<CopyAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallPasteCommandOnPaste()
        {
            Expect.Call(() => _pasteMeetingCommand.Execute());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<PasteAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldUpdateViewOnModificationOccurred()
        {
            Expect.Call(() => _view.ReloadMeetings());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<MeetingModificationOccurred>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldUpdateViewOnSelectionChange()
        {
            var meeting = _mocks.StrictMock<IMeeting>();
            Expect.Call(_deleteMeetingCommand.CanExecute()).Return(true);
            Expect.Call(_view.DeleteEnabled = true);

            Expect.Call(_editMeetingCommand.CanExecute()).Return(true);
            Expect.Call(_view.EditEnabled = true);

            Expect.Call(_copyMeetingCommand.CanExecute()).Return(true);
            Expect.Call(_view.CopyEnabled = true);

            Expect.Call(_pasteMeetingCommand.CanExecute()).Return(true);
            Expect.Call(_view.PasteEnabled = true);

            Expect.Call(_cutMeetingCommand.CanExecute()).Return(true);
            Expect.Call(_view.CutEnabled = true);

            Expect.Call(_addMeetingCommand.CanExecute()).Return(true);
            Expect.Call(_view.AddEnabled = true);

            Expect.Call(_showExportMeetingCommand.CanExecute()).Return(true);
            Expect.Call(_view.ExportEnabled = true);

            Expect.Call(_view.SelectedMeeting).Return(meeting);
            Expect.Call(_infoTextFormatter.GetInfoText(meeting)).Return("a cause for a meeting");
            Expect.Call(() => _view.SetInfoText("a cause for a meeting"));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<AppointmentSelectionChanged>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReloadMeetingsOnOptimisticLockOnDelete()
        {
            Expect.Call(() =>_deleteMeetingCommand.Execute()).Throw(new OptimisticLockException("optimistic lock"));
            Expect.Call(() =>_view.ShowErrorMessage("")).IgnoreArguments();
            Expect.Call(_view.ReloadMeetings);
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<DeleteAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "bidde"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Det"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "fel"), Test]
        public void ShouldShowDataSourceExceptionOnError()
        {
            const string errMess = "Det bidde fel";
            var exception = new DataSourceException(errMess);
            Expect.Call(() => _pasteMeetingCommand.Execute()).Throw(exception);
            Expect.Call(() => _view.ShowDataSourceException(exception)).IgnoreArguments();
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<PasteAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "bidde"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Det"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "fel"), Test]
        public void ShouldShowDataSourceExceptionOnThatErrorOnDelete()
        {
            const string errMess = "Det bidde fel";
            var exception = new DataSourceException(errMess);
            Expect.Call(() => _deleteMeetingCommand.Execute()).Throw(exception);
            Expect.Call(() => _view.ShowDataSourceException(exception)).IgnoreArguments();
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<DeleteAppointmentClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldExecuteShowExportOnEvent()
        {
            Expect.Call(() => _showExportMeetingCommand.Execute());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<ShowExportMeetingClicked>().Publish(string.Empty);
            _mocks.VerifyAll();
        }

		[Test]
		public void ShouldExecuteFetchChangedOnFetchedChangedEvent()
		{
			Expect.Call(() => _fetchMeetingForCurrentUser.Execute());
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<FetchForCurrentUserChanged>().Publish(true);
			_mocks.VerifyAll();
		}
    }

}
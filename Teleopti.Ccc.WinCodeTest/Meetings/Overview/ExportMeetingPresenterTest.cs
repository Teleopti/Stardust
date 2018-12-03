using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
    [TestFixture, System.Runtime.InteropServices.GuidAttribute("19D39AFE-B60D-444C-9CA2-33C99F867114")]
    public class ExportMeetingPresenterTest
    {
        private MockRepository _mocks;
        private IExportMeetingView _exportView;
        private IMeetingOverviewView _meetingOverView;
        private EventAggregator _eventAggregator;
        private ExportMeetingPresenter _target;
        private IExportableScenarioProvider _scenarioProvider;
        private IExportMeetingCommand _exportCommand;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _exportView = _mocks.StrictMock<IExportMeetingView>();
            _meetingOverView = _mocks.StrictMock<IMeetingOverviewView>();
            _eventAggregator = new EventAggregator();
            _scenarioProvider = _mocks.StrictMock<IExportableScenarioProvider>();
            _exportCommand = _mocks.StrictMock<IExportMeetingCommand>();
            _target = new ExportMeetingPresenter(_exportView, _meetingOverView, _eventAggregator,_scenarioProvider, _exportCommand);
        }

        
        [Test]
        public void ShouldCallShowDialogOnViewOnShow()
        {
            var week = new DateOnlyPeriod(2011, 5, 9, 2011, 5, 15);
            var scenarios = new List<IScenario>();
            Expect.Call(_exportView.ProgressBarVisible = false);
            Expect.Call(_exportView.ExportInfoText = string.Empty);
            Expect.Call(_meetingOverView.SelectedWeek).Return(week);
            Expect.Call(_exportView.SelectedDates = new List<DateOnlyPeriod> { week });
            Expect.Call(_scenarioProvider.AllowedScenarios()).Return(scenarios);
            Expect.Call(() => _exportView.SetScenarioList(scenarios));
            Expect.Call(_exportView.ShowDialog(null)).Return(DialogResult.OK).IgnoreArguments();
            _mocks.ReplayAll();
            _target.Show();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldDisableExportIfNoDates()
        {
            Expect.Call(_exportCommand.CanExecute()).Return(false);
            Expect.Call(_exportView.ExportEnabled = false);
            Expect.Call(() => _exportView.SetTextOnDateSelectionFromTo(Resources.StartDateMustBeSmallerThanEndDate));

            _mocks.ReplayAll();
            _eventAggregator.GetEvent<MeetingExportDatesChanged>().Publish(string.Empty);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldEnableExportICanExecute()
        {
            Expect.Call(_exportCommand.CanExecute()).Return(true);
            Expect.Call(_exportView.ExportEnabled = true);
            Expect.Call(() => _exportView.SetTextOnDateSelectionFromTo(string.Empty));

            _mocks.ReplayAll();
            _eventAggregator.GetEvent<MeetingExportDatesChanged>().Publish(string.Empty);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallExportCommandOnExportEvent()
        {
            Expect.Call(_exportView.ProgressBarVisible = true);
            Expect.Call(_exportView.ExportInfoText = string.Empty);
            Expect.Call(() => _exportCommand.Execute());
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<ExportMeetingClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "bidde"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Det"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "fel"), Test]
        public void ShouldShowDataSourceExceptionOnError()
        {
            const string errMess = "Det bidde fel";
            var exception = new DataSourceException(errMess);
            Expect.Call(_exportView.ProgressBarVisible = true);
            Expect.Call(_exportView.ExportInfoText = string.Empty);
            Expect.Call(() => _exportCommand.Execute()).Throw(exception);
            Expect.Call(_exportView.ProgressBarVisible = false);
            Expect.Call(_exportView.ExportInfoText = exception.Message);
            Expect.Call(() => _exportView.ShowDataSourceException(exception));
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<ExportMeetingClicked>().Publish("");
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetInfoAndHideProgressBarWhenFinished()
        {
            Expect.Call(_exportView.ProgressBarVisible = false);
            Expect.Call(_exportView.ExportInfoText = "10 meetings exported").IgnoreArguments();
            _mocks.ReplayAll();
            _eventAggregator.GetEvent<MeetingExportFinished>().Publish(10);
            _mocks.ReplayAll();
        }
    }

}
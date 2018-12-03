using System.Collections.Generic;
using System.Globalization;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview
{
    public interface IExportMeetingPresenter
    {
        void Show();
    }

    public class ExportMeetingPresenter : IExportMeetingPresenter
    {
        private readonly IExportMeetingView _exportMeetingView;
        private readonly IMeetingOverviewView _overviewView;
        private readonly IEventAggregator _eventAggregator;
        private readonly IExportableScenarioProvider _exportableScenarioProvider;
        private readonly IExportMeetingCommand _exportMeetingCommand;

        public ExportMeetingPresenter(IExportMeetingView exportMeetingView, IMeetingOverviewView overviewView, 
            IEventAggregator eventAggregator, IExportableScenarioProvider exportableScenarioProvider, IExportMeetingCommand exportMeetingCommand)
        {
            _exportMeetingView = exportMeetingView;
            _overviewView = overviewView;
            _eventAggregator = eventAggregator;
            _exportableScenarioProvider = exportableScenarioProvider;
            _exportMeetingCommand = exportMeetingCommand;

            _eventAggregator.GetEvent<MeetingExportDatesChanged>().Subscribe(meetingExportDatesChanged);
            _eventAggregator.GetEvent<ExportMeetingClicked>().Subscribe(export);
            _eventAggregator.GetEvent<MeetingExportFinished>().Subscribe(exportFinished);
        }

        public void Show()
        {
            _exportMeetingView.ProgressBarVisible = false;
            _exportMeetingView.ExportInfoText = string.Empty;
            //s�tt startdatum och ladda scenariocombo f�re visning
            
            _exportMeetingView.SelectedDates = new List<DateOnlyPeriod> { _overviewView.SelectedWeek };
            _exportMeetingView.SetScenarioList(_exportableScenarioProvider.AllowedScenarios());
            _exportMeetingView.ShowDialog(_overviewView);
        }

        private void meetingExportDatesChanged(string something)
        {
            var can = _exportMeetingCommand.CanExecute();
            _exportMeetingView.ExportEnabled = can;

            _exportMeetingView.SetTextOnDateSelectionFromTo(can ? string.Empty : Resources.StartDateMustBeSmallerThanEndDate);
        }

        private void export(string something)
        {
            _exportMeetingView.ProgressBarVisible = true;
            _exportMeetingView.ExportInfoText = string.Empty;
            try
            {
                _exportMeetingCommand.Execute();
            }
            catch (DataSourceException dataSourceException)
            {
                _exportMeetingView.ProgressBarVisible = false;
                _exportMeetingView.ExportInfoText = dataSourceException.Message;
                _exportMeetingView.ShowDataSourceException(dataSourceException);
            }
            
        }

        private void exportFinished(int numberOfMeetings)
        {
            _exportMeetingView.ProgressBarVisible = false;
            _exportMeetingView.ExportInfoText = string.Format(CultureInfo.CurrentCulture, Resources.SuccessfullyExportedMeetings, numberOfMeetings);
        }
        
    }
}
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Meetings.Commands;
using Teleopti.Ccc.WinCode.Meetings.Events;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Overview
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
            //sätt startdatum och ladda scenariocombo före visning
            
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
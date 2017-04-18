using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview
{
    public class MeetingOverviewPresenter
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IInfoWindowTextFormatter _infoWindowTextFormatter;
        private readonly IMeetingOverviewView _meetingOverviewView;
        private readonly IDeleteMeetingCommand _deleteMeetingCommand;
        private readonly IAddMeetingCommand _addMeetingCommand;
        private readonly IEditMeetingCommand _editMeetingCommand;
        private readonly ICopyMeetingCommand _copyMeetingCommand;
        private readonly IPasteMeetingCommand _pasteMeetingCommand;
        private readonly IShowExportMeetingCommand _showExportMeetingCommand;
    	private readonly IFetchMeetingForCurrentUserChangeCommand _fetchMeetingForCurrentUserChangeCommand;
    	private readonly ICutMeetingCommand _cutMeetingCommand;

        public MeetingOverviewPresenter(IMeetingOverviewView meetingOverviewView, IEventAggregator eventAggregator, IInfoWindowTextFormatter infoWindowTextFormatter,
            IDeleteMeetingCommand deleteMeetingCommand, IAddMeetingCommand addMeetingCommand, IEditMeetingCommand editMeetingCommand,
            ICutMeetingCommand cutMeetingCommand, ICopyMeetingCommand copyMeetingCommand, IPasteMeetingCommand pasteMeetingCommand, 
			IShowExportMeetingCommand showExportMeetingCommand, IFetchMeetingForCurrentUserChangeCommand fetchMeetingForCurrentUserChangeCommand)
        {
            _meetingOverviewView = meetingOverviewView;
            _eventAggregator = eventAggregator;
            _infoWindowTextFormatter = infoWindowTextFormatter;
            _deleteMeetingCommand = deleteMeetingCommand;
            _addMeetingCommand = addMeetingCommand;
            _editMeetingCommand = editMeetingCommand;
            _cutMeetingCommand = cutMeetingCommand;
            _copyMeetingCommand = copyMeetingCommand;
            _pasteMeetingCommand = pasteMeetingCommand;
            _showExportMeetingCommand = showExportMeetingCommand;
        	_fetchMeetingForCurrentUserChangeCommand = fetchMeetingForCurrentUserChangeCommand;
        	if(_eventAggregator != null)
            {
                _eventAggregator.GetEvent<DeleteAppointmentClicked>().Subscribe(deleteAppointment);
                _eventAggregator.GetEvent<AddAppointmentClicked>().Subscribe(addAppointment);
                _eventAggregator.GetEvent<EditAppointmentClicked>().Subscribe(editAppointment);
                _eventAggregator.GetEvent<AppointmentSelectionChanged>().Subscribe(updateView);
                _eventAggregator.GetEvent<MeetingModificationOccurred>().Subscribe(reloadMeetings);
                _eventAggregator.GetEvent<CopyAppointmentClicked>().Subscribe(copyMeeting);
                _eventAggregator.GetEvent<PasteAppointmentClicked>().Subscribe(pasteMeeting);
                _eventAggregator.GetEvent<CutAppointmentClicked>().Subscribe(cutMeeting);
                _eventAggregator.GetEvent<ShowExportMeetingClicked>().Subscribe(showExportForm);
            	_eventAggregator.GetEvent<FetchForCurrentUserChanged>().Subscribe(fetchForCurrentUserChanged);
            }
            
        }

    	private void fetchForCurrentUserChanged(bool obj)
    	{
    		_fetchMeetingForCurrentUserChangeCommand.Execute();
    	}

    	private void deleteAppointment(string something)
        {
            try
            {
                _deleteMeetingCommand.Execute();
            }
            catch (OptimisticLockException)
            {
                _meetingOverviewView.ShowErrorMessage(Resources.SomeoneChangedTheSameDataBeforeYouDot);
                _meetingOverviewView.ReloadMeetings();
            }
            catch (DataSourceException dataSourceException)
            {
                _meetingOverviewView.ShowDataSourceException(dataSourceException);
            }
        }

        private void addAppointment(string something)
        {
             executeCommand(_addMeetingCommand);
        }

        private void editAppointment(string something)
        {
            executeCommand(_editMeetingCommand);
        }

        private void copyMeeting(string something)
        {
            _copyMeetingCommand.Execute();
        }

        private void pasteMeeting(string something)
        {
            executeCommand(_pasteMeetingCommand);   
        }

        private void cutMeeting(string something)
        {
            executeCommand(_cutMeetingCommand);
        }

        private void showExportForm(string something)
        {
            executeCommand(_showExportMeetingCommand);
        }

        private void executeCommand(IExecutableCommand command)
        {
            try
            {
                command.Execute();
            }
            catch (DataSourceException dataSourceException)
            {
                _meetingOverviewView.ShowDataSourceException(dataSourceException);
            }
        }

        private void updateView(string something)
        {
            _meetingOverviewView.DeleteEnabled = _deleteMeetingCommand.CanExecute();
            _meetingOverviewView.EditEnabled = _editMeetingCommand.CanExecute();
            _meetingOverviewView.CopyEnabled = _copyMeetingCommand.CanExecute();
            _meetingOverviewView.PasteEnabled = _pasteMeetingCommand.CanExecute();
            _meetingOverviewView.CutEnabled = _cutMeetingCommand.CanExecute();
            _meetingOverviewView.AddEnabled = _addMeetingCommand.CanExecute();
            _meetingOverviewView.ExportEnabled = _showExportMeetingCommand.CanExecute();
            var meeting = _meetingOverviewView.SelectedMeeting;
            //eller tomt
            var text = Resources.NoMeetingIsSelected;
            if (meeting != null)
            {
                text = _infoWindowTextFormatter.GetInfoText(meeting);
            }
            
          _meetingOverviewView.SetInfoText(text);
        }

        private void reloadMeetings(string something)
        {
            _meetingOverviewView.ReloadMeetings();
        }
    }
}
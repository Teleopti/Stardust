using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
    public interface IPasteMeetingCommand : IExecutableCommand, ICanExecute
    {
    }

    public class PasteMeetingCommand : IPasteMeetingCommand
    {
        private readonly IMeetingClipboardHandler _meetingClipboardHandler;
        private readonly IMeetingOverviewView _meetingOverviewView;
        private readonly IMeetingChangerAndPersister _meetingChangerAndPersister;
        private readonly ICanModifyMeeting _canModifyMeeting;

        public PasteMeetingCommand(IMeetingClipboardHandler meetingClipboardHandler, IMeetingOverviewView meetingOverviewView, 
            IMeetingChangerAndPersister meetingChangerAndPersister, ICanModifyMeeting canModifyMeeting)
        {
            _meetingClipboardHandler = meetingClipboardHandler;
            _meetingOverviewView = meetingOverviewView;
            _meetingChangerAndPersister = meetingChangerAndPersister;
            _canModifyMeeting = canModifyMeeting;
        }

        public void Execute()
        {
            if(!CanExecute())
                return;
            var meeting = _meetingClipboardHandler.GetData();
            
            var pasteTo = _meetingOverviewView.SelectedPeriod();

            _meetingChangerAndPersister.ChangeStartDateTimeAndPersist(meeting, pasteTo.StartDateTime, TimeSpan.FromMinutes(0), _meetingOverviewView.UserTimeZone, _meetingOverviewView);
            
            _meetingOverviewView.ReloadMeetings();
        }

        public bool CanExecute()
        {
        	if (_meetingOverviewView.SelectedPeriod().EndDateTime.Equals(DateTime.MinValue))
        		return false;
        	if (!_canModifyMeeting.CanExecute)
                return false;
            return _meetingClipboardHandler.HasData();
        }
    }
}
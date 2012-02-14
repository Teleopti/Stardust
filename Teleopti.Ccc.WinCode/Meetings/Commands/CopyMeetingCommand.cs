using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
    public interface ICopyMeetingCommand : IExecutableCommand, ICanExecute
    {
    }

    public class CopyMeetingCommand : ICopyMeetingCommand
    {
        private readonly IMeetingClipboardHandler _meetingClipboardHandler;
        private readonly IMeetingOverviewView _meetingOverviewView;
        private readonly ICanModifyMeeting _canModifyMeeting;

        public CopyMeetingCommand(IMeetingClipboardHandler meetingClipboardHandler, IMeetingOverviewView meetingOverviewView,
            ICanModifyMeeting canModifyMeeting)
        {
            _meetingClipboardHandler = meetingClipboardHandler;
            _meetingOverviewView = meetingOverviewView;
            _canModifyMeeting = canModifyMeeting;
        }

        public void Execute()
        {
            if(CanExecute())
                _meetingClipboardHandler.SetData(_meetingOverviewView.SelectedMeeting);
        }

        public bool CanExecute()
        {
            var meeting = _meetingOverviewView.SelectedMeeting;
            if (meeting == null)
                return false;
            if (meeting.GetRecurringDates().Count > 1)
                return false;
            return _canModifyMeeting.CanExecute;
        }
    }
}
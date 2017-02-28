using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
    public interface ICutMeetingCommand : IExecutableCommand, ICanExecute
    {
    }
    public class CutMeetingCommand: ICutMeetingCommand
    {
        private readonly IMeetingClipboardHandler _meetingClipboardHandler;
        private readonly IMeetingOverviewView _meetingOverviewView;
        private readonly IMeetingRepository _meetingRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICanModifyMeeting _canModifyMeeting;

        public CutMeetingCommand(IMeetingClipboardHandler meetingClipboardHandler, IMeetingOverviewView meetingOverviewView,
            IMeetingRepository meetingRepository, IUnitOfWorkFactory unitOfWorkFactory, ICanModifyMeeting canModifyMeeting)
        {
            _meetingClipboardHandler = meetingClipboardHandler;
            _meetingOverviewView = meetingOverviewView;
            _meetingRepository = meetingRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _canModifyMeeting = canModifyMeeting;
        }

        public void Execute()
        {
            if (!CanExecute())
                return;
            var meeting = _meetingOverviewView.SelectedMeeting;
            _meetingClipboardHandler.SetData(meeting);

            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
				meeting.Snapshot();

                _meetingRepository.Remove(meeting);
                unitOfWork.PersistAll();
                _meetingOverviewView.ReloadMeetings();
            }
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
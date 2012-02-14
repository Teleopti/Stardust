using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
    public interface IDeleteMeetingCommand : IExecutableCommand, ICanExecute
    {
    }

    public class DeleteMeetingCommand : IDeleteMeetingCommand
    {
        private readonly IMeetingOverviewView _meetingOverviewView;
        private readonly IMeetingRepository _meetingRepository;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICanModifyMeeting _canModifyMeeting;

        public DeleteMeetingCommand(IMeetingOverviewView meetingOverviewView, IMeetingRepository meetingRepository,
            IUnitOfWorkFactory unitOfWorkFactory, ICanModifyMeeting canModifyMeeting)
        {
            _meetingOverviewView = meetingOverviewView;
            _meetingRepository = meetingRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _canModifyMeeting = canModifyMeeting;
        }

        public void Execute()
        {
            if(!CanExecute())
                return;
            var theMeeting = _meetingOverviewView.SelectedMeeting;
            
            if (!_meetingOverviewView.ConfirmDeletion(theMeeting))
                return;

            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
				theMeeting.Snapshot();

                _meetingRepository.Remove(theMeeting);
                unitOfWork.PersistAll();
                _meetingOverviewView.ReloadMeetings();
            }
        }

        public bool CanExecute()
        {
            if (!_canModifyMeeting.CanExecute)
                return false;
            return _meetingOverviewView.SelectedMeeting != null;
        }
    }
}
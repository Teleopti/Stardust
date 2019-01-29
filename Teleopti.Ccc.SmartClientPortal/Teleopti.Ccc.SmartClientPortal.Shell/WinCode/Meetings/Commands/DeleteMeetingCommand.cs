using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands
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
        private readonly IMeetingParticipantPermittedChecker _meetingParticipantPermittedChecker;

        public DeleteMeetingCommand(IMeetingOverviewView meetingOverviewView, IMeetingRepository meetingRepository,
            IUnitOfWorkFactory unitOfWorkFactory, ICanModifyMeeting canModifyMeeting, IMeetingParticipantPermittedChecker meetingParticipantPermittedChecker)
        {
            _meetingOverviewView = meetingOverviewView;
            _meetingRepository = meetingRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _canModifyMeeting = canModifyMeeting;
            _meetingParticipantPermittedChecker = meetingParticipantPermittedChecker;
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
                var persons = theMeeting.MeetingPersons.Select(m => m.Person);
                unitOfWork.Reassociate(persons);
                if (!_meetingParticipantPermittedChecker.ValidatePermittedPersons(persons, theMeeting.StartDate, _meetingOverviewView, PrincipalAuthorization.Current_DONTUSE()))
                    return;
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
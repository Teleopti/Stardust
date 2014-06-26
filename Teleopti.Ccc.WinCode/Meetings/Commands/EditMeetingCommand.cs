using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings.Commands
{
    public interface IEditMeetingCommand : IExecutableCommand, ICanExecute
    {
    }

    public class EditMeetingCommand : IEditMeetingCommand
    {
        private readonly IMeetingOverviewView _meetingOverviewView;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICanModifyMeeting _canModifyMeeting;
        private readonly IToggleManager _toggleManager;
        private readonly ISettingDataRepository _settingDataRepository;

        public EditMeetingCommand(IMeetingOverviewView meetingOverviewView, ISettingDataRepository settingDataRepository,
            IUnitOfWorkFactory unitOfWorkFactory, ICanModifyMeeting canModifyMeeting, IToggleManager toggleManager)
        {
            _meetingOverviewView = meetingOverviewView;
            _settingDataRepository = settingDataRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _canModifyMeeting = canModifyMeeting;
            _toggleManager = toggleManager;
        }

        public void Execute()
        {
            if(!CanExecute())
                return;
            
            MeetingViewModel meetingViewModel;
            
            var theMeeting = _meetingOverviewView.SelectedMeeting;
            
            var persons = theMeeting.MeetingPersons.Select(meetingPerson => meetingPerson.Person).ToList();
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
				unitOfWork.Reassociate(persons);
                var commonNameDescription = _settingDataRepository.FindValueByKey("CommonNameDescription",
                                                                                        new CommonNameDescriptionSetting());
                
                meetingViewModel = new MeetingViewModel(theMeeting, commonNameDescription);
            }
            _meetingOverviewView.EditMeeting(meetingViewModel, _toggleManager);
        }

        public bool CanExecute()
        {
            if (!_canModifyMeeting.CanExecute)
                return false;
           return _meetingOverviewView.SelectedMeeting != null;
        }
    }
}
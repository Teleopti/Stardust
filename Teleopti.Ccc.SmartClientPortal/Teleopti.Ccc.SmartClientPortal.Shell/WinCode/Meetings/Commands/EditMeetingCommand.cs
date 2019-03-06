using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands
{
    public interface IEditMeetingCommand : IExecutableCommand, ICanExecute
    {
    }

    public class EditMeetingCommand : IEditMeetingCommand
    {
        private readonly IMeetingOverviewView _meetingOverviewView;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ICanModifyMeeting _canModifyMeeting;
	    private readonly IIntraIntervalFinderService _intraIntervalFinderService;
	    private readonly ISkillPriorityProvider _skillPriorityProvider;
		private readonly ISettingDataRepository _settingDataRepository;

        public EditMeetingCommand(IMeetingOverviewView meetingOverviewView, ISettingDataRepository settingDataRepository,
            IUnitOfWorkFactory unitOfWorkFactory, ICanModifyMeeting canModifyMeeting, IIntraIntervalFinderService intraIntervalFinderService, ISkillPriorityProvider skillPriorityProvider)
        {
            _meetingOverviewView = meetingOverviewView;
            _settingDataRepository = settingDataRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _canModifyMeeting = canModifyMeeting;
	        _intraIntervalFinderService = intraIntervalFinderService;
	        _skillPriorityProvider = skillPriorityProvider;
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
            _meetingOverviewView.EditMeeting(meetingViewModel, _intraIntervalFinderService, _skillPriorityProvider);
        }

        public bool CanExecute()
        {
            if (!_canModifyMeeting.CanExecute)
                return false;
           return _meetingOverviewView.SelectedMeeting != null;
        }
    }
}
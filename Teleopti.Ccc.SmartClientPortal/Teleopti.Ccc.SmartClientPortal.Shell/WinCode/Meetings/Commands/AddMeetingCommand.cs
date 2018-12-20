using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands
{
    public interface IAddMeetingCommand : IExecutableCommand, ICanExecute
    {
    }

    public class AddMeetingCommand : IAddMeetingCommand
    {
        private readonly IMeetingOverviewView _meetingOverviewView;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IMeetingOverviewViewModel _model;
        private readonly ICanModifyMeeting _canModifyMeeting;
	    private readonly IIntraIntervalFinderService _intraIntervalFinderService;
	    private readonly ISkillPriorityProvider _skillPriorityProvider;
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;
		private readonly ISettingDataRepository _settingDataRepository;
        private readonly IActivityRepository _activityRepository;
        private readonly IPersonRepository _personRepository;

        public AddMeetingCommand(IMeetingOverviewView meetingOverviewView, ISettingDataRepository settingDataRepository,
            IActivityRepository activityRepository, IPersonRepository personRepository, ICurrentUnitOfWorkFactory unitOfWorkFactory, IMeetingOverviewViewModel model,
            ICanModifyMeeting canModifyMeeting, IIntraIntervalFinderService intraIntervalFinderService, ISkillPriorityProvider skillPriorityProvider, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
        {
            _meetingOverviewView = meetingOverviewView;
            _activityRepository = activityRepository;
            _personRepository = personRepository;
            _settingDataRepository = settingDataRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _model = model;
            _canModifyMeeting = canModifyMeeting;
	        _intraIntervalFinderService = intraIntervalFinderService;
	        _skillPriorityProvider = skillPriorityProvider;
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
		}

        public void Execute()
        {
            if(!CanExecute())
                return;
            MeetingViewModel meetingViewModel = null;
            IScenario selectedScenario = _model.CurrentScenario;
            
            IList<IActivity> activities;
            using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				IPerson organizer = _personRepository.Get(TeleoptiPrincipalForLegacy.CurrentPrincipal.PersonId);
                activities = _activityRepository.LoadAllSortByName();

                var commonNameDescription = _settingDataRepository.FindValueByKey("CommonNameDescription",
                                                                                        new CommonNameDescriptionSetting());
                //uow.Reassociate(selectedPersons);
                if ((activities != null) && (activities.Count > 0))
                {
                    var period = _meetingOverviewView.SelectedPeriod();
                    meetingViewModel = MeetingComposerPresenter.CreateDefaultMeeting(organizer, selectedScenario,
                                                                                    activities[0], new DateOnly(period.StartDateTime),
                                                                                    new List<IPerson>(), commonNameDescription,
                                                                                    organizer.PermissionInformation.DefaultTimeZone(), new Now());
                    meetingViewModel.StartDate = new DateOnly(period.StartDateTime);
                    meetingViewModel.StartTime = period.StartDateTime.TimeOfDay;
                    meetingViewModel.MeetingDuration = period.EndDateTime - period.StartDateTime;
                }

                if (meetingViewModel != null)
                    _meetingOverviewView.EditMeeting(meetingViewModel, _intraIntervalFinderService, _skillPriorityProvider, _staffingCalculatorServiceFacade);
            }
        }

        public bool CanExecute()
        {
            var period = _meetingOverviewView.SelectedPeriod();
            if (period.StartDateTime.Equals(System.DateTime.MinValue))
                return false;
            return _canModifyMeeting.CanExecute;
        }
    }
}
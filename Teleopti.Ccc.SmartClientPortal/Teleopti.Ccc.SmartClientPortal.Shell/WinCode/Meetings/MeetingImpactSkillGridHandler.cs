using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
    public interface IMeetingImpactSkillGridHandler
    {
        void SetupSkillTabs();
        void DrawSkillGrid();
    }

    public class MeetingImpactSkillGridHandler : IMeetingImpactSkillGridHandler
    {
        private readonly ISchedulerStateHolder _schedulerStateHolder;
        private readonly IMeetingImpactView _meetingImpactView;
        private readonly IMeetingViewModel _meetingViewModel;
        private readonly IList<ISkill> _skills = new List<ISkill>();
        private readonly IPeopleAndSkillLoaderDecider _decider;
		private readonly IUnitOfWorkFactory _uowFactory;
        IList<ISkillStaffPeriod> _skillStaffPeriods = new List<ISkillStaffPeriod>();
        private ISchedulingResultStateHolder _schedulingResultStateHolder;

        public MeetingImpactSkillGridHandler(IMeetingImpactView meetingImpactView, IMeetingViewModel meetingViewModel, ISchedulerStateHolder schedulerStateHolder,
            IUnitOfWorkFactory uowFactory, IPeopleAndSkillLoaderDecider decider)
        {
            _meetingImpactView = meetingImpactView;
            _meetingViewModel = meetingViewModel;
            _schedulerStateHolder = schedulerStateHolder;
            _uowFactory = uowFactory;
            _decider = decider;
		}
        private ISchedulingResultStateHolder SchedulingResultStateHolder()
        {
            return _schedulingResultStateHolder ??
                   (_schedulingResultStateHolder = _schedulerStateHolder.SchedulingResultState);
        }

        public void SetupSkillTabs()
        {
            _meetingImpactView.RemoveAllSkillTabs();
            _skills.Clear();
            foreach (var skill in SchedulingResultStateHolder().Skills)
            {
                _skills.Add(skill);
            }
            using (_uowFactory.CreateAndOpenUnitOfWork())
            {
            	var result = _decider.Execute(_schedulerStateHolder.RequestedScenario, _schedulerStateHolder.RequestedPeriod.Period(),
            	                 _meetingViewModel.Meeting.MeetingPersons.Select(p => p.Person).ToList());

            	result.FilterSkills(_skills.ToArray(),s => _skills.Remove(s),_skills.Add);
            }

            _meetingImpactView.ClearTabPages();
            foreach (var skill in _skills.OrderBy(s => s.Name))
            {
                _meetingImpactView.AddSkillTab(skill.Name, skill.Description, ImageIndexSkillType(skill.SkillType.ForecastSource), skill);
            }

            // TODO efter detta anrop k�r koden nedan
            //_meetingImpactCalculator.RecalculateResources(_meetingImpactView.StartDate);
        }

        private static int ImageIndexSkillType(ForecastSource skillType)
        {
            switch (skillType)
            {
                case ForecastSource.Email:
                    return 0;
                case ForecastSource.Facsimile:
                    return 1;
                case ForecastSource.Backoffice:
                    return 3;
                default:
                    return 2;
            }
        }

        public void DrawSkillGrid()
        {
            var skill = _meetingImpactView.SelectedSkill();

            if (skill != null)
            {
                var currentIntradayDate = _meetingImpactView.StartDate;
				var periodToFind = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentIntradayDate.Date, currentIntradayDate.AddDays(1).Date, TimeZoneGuardForDesktop_DONOTUSE.Instance_DONTUSE.CurrentTimeZone());
                    
                if (skill.IsVirtual)
                {
                    _skillStaffPeriods = SchedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodList(skill, periodToFind);
                }
                else
                {
                    _skillStaffPeriods = SchedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, periodToFind);
                }
                if (_skillStaffPeriods.Count >= 0)
                {
                    _meetingImpactView.DrawIntraday(skill, _schedulerStateHolder, _skillStaffPeriods);
                    _meetingImpactView.ClearTabPages();
                    _meetingImpactView.PositionControl();
                }
            }
        }
    }
}
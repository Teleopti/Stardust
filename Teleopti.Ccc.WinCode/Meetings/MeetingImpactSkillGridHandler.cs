using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Meetings
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
            	_decider.Execute(_schedulerStateHolder.RequestedScenario, _schedulerStateHolder.RequestedPeriod.Period(),
            	                 _meetingViewModel.Meeting.MeetingPersons.Select(p => p.Person).ToList());

            	_decider.FilterSkills(_skills);
            }

            _meetingImpactView.ClearTabPages();
            foreach (var skill in _skills.OrderBy(s => s.Name))
            {
                _meetingImpactView.AddSkillTab(skill.Name, skill.Description, ImageIndexSkillType(skill.SkillType.ForecastSource), skill);
            }

            // TODO efter detta anrop kör koden nedan
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

                if (skill.IsVirtual)
                {
                    _skillStaffPeriods = SchedulingResultStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodList(skill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentIntradayDate, currentIntradayDate.AddDays(1), _schedulerStateHolder.TimeZoneInfo));
                }
                else
                {
                    var periodToFind = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentIntradayDate, currentIntradayDate.AddDays(1), _schedulerStateHolder.TimeZoneInfo);
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
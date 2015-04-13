using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public interface ISchedulerSkillDayHelper
    {
		void AddSkillDaysToStateHolder(DateOnlyPeriod datePeriod, ForecastSource forecastSource, int demand);
    }

    public class SchedulerSkillDayHelper : ISchedulerSkillDayHelper
    {
        private readonly ISchedulerStateHolder _schedulerStateHolder;
        private readonly ISkillDayRepository _skillDayRepository;

        public SchedulerSkillDayHelper(ISchedulerStateHolder schedulerStateHolder, ISkillDayRepository skillDayRepository)
        {
            _schedulerStateHolder = schedulerStateHolder;
            _skillDayRepository = skillDayRepository;
        }

        public void AddSkillDaysToStateHolder(DateOnlyPeriod datePeriod, ForecastSource forecastSource, int demand)
        {
            var theSkillDays = _schedulerStateHolder.SchedulingResultState.SkillDays;
            // TODO remove first
            foreach (var skill in _schedulerStateHolder.SchedulingResultState.Skills)
            {
                if (skill.SkillType.ForecastSource == forecastSource)
                {
                    ICollection<ISkillDay> skillDays =
                    _skillDayRepository.GetAllSkillDays(datePeriod, new List<ISkillDay>(), skill,
                                                       _schedulerStateHolder.RequestedScenario, _ => {});
                    foreach (ISkillDay skillDay in skillDays)
                    {
                        var sDay = skillDay as IMaxSeatSkillDay;
                        if (sDay != null)
                            sDay.OpenAllSkillStaffPeriods();
                        foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
                        {
                            var payload = skillStaffPeriod.Payload;
							if (forecastSource == ForecastSource.NonBlendSkill)
							{
								payload.NoneBlendDemand = demand;
								
								payload.ServiceAgreementData = new ServiceAgreement(new ServiceLevel(new Percent(1), 1), new Percent(0),
								                                                    new Percent(1));
								skillStaffPeriod.CalculateStaff();
							}
                        }
                    }
                    theSkillDays.Add(skill, new List<ISkillDay>(skillDays));
                }
            }

            _schedulerStateHolder.SchedulingResultState.SkillDays = theSkillDays;
        }
    }
}
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public interface ISchedulerSkillDayHelper
    {
        void AddSkillDaysToStateHolder(ForecastSource forecastSource, int demand);
    }

    public class SchedulerSkillDayHelper : ISchedulerSkillDayHelper
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly DateOnlyPeriod _dateTimePeriod;
        private readonly ISkillDayRepository _skillDayRepository;
        private readonly IScenario _scenario;

        public SchedulerSkillDayHelper(ISchedulingResultStateHolder schedulingResultStateHolder, DateOnlyPeriod dateTimePeriod,
                ISkillDayRepository skillDayRepository, IScenario scenario)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _dateTimePeriod = dateTimePeriod;
            _skillDayRepository = skillDayRepository;
            _scenario = scenario;
        }

        public void AddSkillDaysToStateHolder(ForecastSource forecastSource, int demand)
        {
            var theSkillDays = _schedulingResultStateHolder.SkillDays;
            // TODO remove first
            foreach (var skill in _schedulingResultStateHolder.Skills)
            {
                if (skill.SkillType.ForecastSource == forecastSource)
                {
                    ICollection<ISkillDay> skillDays =
                    _skillDayRepository.GetAllSkillDays(_dateTimePeriod, new List<ISkillDay>(), skill,
                                                       _scenario, _ => {});
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

            _schedulingResultStateHolder.SkillDays = theSkillDays;
        }
    }
}
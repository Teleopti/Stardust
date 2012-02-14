using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Mapps a skill day
    /// </summary>
    public class SkillDayMapper : Mapper<ISkillDay, global::Domain.SkillDay>
    {
        private int _intervalLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDayMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 4.12.2007
        /// </remarks>
        public SkillDayMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone, int intervalLength) : base(mappedObjectPair, timeZone)
        {
            _intervalLength = intervalLength;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 3.12.2007
        /// </remarks>
        public override ISkillDay Map(global::Domain.SkillDay oldEntity)
        {
            SkillDay newSkillDay;

            IList<IWorkloadDay> workloadDays = new List<IWorkloadDay>();
            WorkloadDayMapper workloadDayMapper = new WorkloadDayMapper(MappedObjectPair, TimeZone, _intervalLength);
            foreach (global::Domain.ForecastDay forecastDay in oldEntity.ForecastDayCollection.Values)
            {
                workloadDays.Add(workloadDayMapper.Map(forecastDay));
            }

            DateTime dateTime = TimeZone.ConvertTimeToUtc(oldEntity.SkillDate, TimeZone);
            IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
            SkillDataPeriodMapper skillDataPeriodMapper = new SkillDataPeriodMapper(MappedObjectPair, TimeZone, dateTime, _intervalLength);
            foreach (global::Domain.SkillData skillData in oldEntity.SkillDataCollection.Values)
            {
                skillDataPeriods.Add(skillDataPeriodMapper.Map(skillData));
            }

            newSkillDay=new SkillDay(
                new DateOnly(oldEntity.SkillDate), 
                MappedObjectPair.Skill.GetPaired(oldEntity.ThisSkill),
                MappedObjectPair.Scenario.GetPaired(oldEntity.SkillScenario),
                workloadDays,
                skillDataPeriods);

            //Template should always be reset to <NONE> for converted days
            newSkillDay.UpdateTemplateName();
            
            return newSkillDay;
        }
    }
}

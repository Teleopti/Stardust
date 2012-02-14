using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-02
    /// </remarks>

    public class SkillAgentDataPeriodMapper :  Mapper<SkillPersonDataPeriod, global::Domain.SkillData>
    {
        private readonly DateTime _date;
        private readonly int _intervalLength;
        private readonly IDictionary<global::Domain.SkillData, global::Domain.Skill> _parentMapping;


        /// <summary>
        /// Initializes a new instance of the <see cref="SkillAgentDataPeriodMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="date">The date.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <param name="parentMapping">The parent mapping.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public SkillAgentDataPeriodMapper(MappedObjectPair mappedObjectPair,
                                TimeZoneInfo timeZone,
                                DateTime date,
                                int intervalLength,
                                IDictionary<global::Domain.SkillData, global::Domain.Skill> parentMapping)
            : base(mappedObjectPair, timeZone)
        {
            _date = date;
            _intervalLength = intervalLength;
            _parentMapping = parentMapping;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public override SkillPersonDataPeriod Map(global::Domain.SkillData oldEntity)
        {
            SkillAgentDataMapper skillAgentDataMapper = new SkillAgentDataMapper();
            SkillPersonData newSkillPersonData = skillAgentDataMapper.Map(oldEntity);

            DateTimePeriodMapper dtpMap = new DateTimePeriodMapper(TimeZone, _date);

            TimeSpan ts = new TimeSpan(0, _intervalLength * oldEntity.Interval, 0);
            TimeSpan endTs =
                new TimeSpan(0, _intervalLength * oldEntity.Interval + _intervalLength, 0);

            global::Domain.TimePeriod tp = new global::Domain.TimePeriod(ts, endTs);

            DateTimePeriod newPeriod = dtpMap.Map(tp);   

            Skill belongingSkill = MappedObjectPair.Skill.GetPaired(_parentMapping[oldEntity]);
            return new SkillPersonDataPeriod(belongingSkill, newSkillPersonData, newPeriod);
        }
    }
}
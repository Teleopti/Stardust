using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2007-11-02
    /// </remarks>
    public class SkillDataPeriodMapper : Mapper<SkillDataPeriod, global::Domain.SkillData>
    {
        private readonly int _intervalLength;
        private readonly DateTime _dateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillDataPeriodMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="intervalLength">Length of the interval.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-02
        /// </remarks>
        public SkillDataPeriodMapper(MappedObjectPair mappedObjectPair,
                                ICccTimeZoneInfo timeZone,
                                DateTime dateTime,
                                int intervalLength)
            : base(mappedObjectPair, timeZone)
        {
            _intervalLength = intervalLength;
            _dateTime = dateTime;
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
        public override SkillDataPeriod Map(global::Domain.SkillData oldEntity)
        {
            SkillDataMapper skillDataMapper = new SkillDataMapper();
            ServiceAgreement newServiceAgreement = skillDataMapper.Map(oldEntity);

            TimeSpan ts = new TimeSpan(0, _intervalLength * oldEntity.Interval, 0);
            TimeSpan endTs =
                new TimeSpan(0, _intervalLength * oldEntity.Interval + _intervalLength, 0);

            DateTimePeriod newPeriod = new DateTimePeriod(_dateTime.Add(ts), _dateTime.Add(endTs));

            int max = oldEntity.MaxStaff;
            if (max < oldEntity.MinStaff) max = 0;
            SkillPersonData skillPersonData = new SkillPersonData(oldEntity.MinStaff, max);
            SkillDataPeriod skillDataPeriod = new SkillDataPeriod(newServiceAgreement, skillPersonData, newPeriod);

            //Convert old shrinkage from double to percent
            if (oldEntity.Shrinkage > 1)
                skillDataPeriod.Shrinkage = new Percent(oldEntity.Shrinkage - 1);
            else
                skillDataPeriod.Shrinkage = new Percent(0);

            return skillDataPeriod;
        }
    }
}
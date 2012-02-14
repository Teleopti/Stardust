using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Mapps old Preferences to new PersonRestrictions
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-12-03
    /// </remarks>
    public class PreferenceMapper : Mapper<IPreferenceDay, global::Domain.AgentDayPreference>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferenceMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-12-03
        /// </remarks>
        public PreferenceMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone)
            : base(mappedObjectPair, timeZone)
        {
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 10/23/2007
        /// </remarks>
        public override IPreferenceDay Map(global::Domain.AgentDayPreference oldEntity)
        {
            IPreferenceRestriction mainRestriction = new PreferenceRestriction();

            if (oldEntity.PreferenceAbsence != null && oldEntity.PreferenceAbsence.UseCountRules)
            {
                //TODO! How is this suppose to work?
                mainRestriction.DayOffTemplate = MappedObjectPair.DayOff.GetPaired(oldEntity.PreferenceAbsence);
            }
            if (oldEntity.ShiftCategory != null)
                mainRestriction.ShiftCategory = MappedObjectPair.ShiftCategory.GetPaired(oldEntity.ShiftCategory);

            if (oldEntity.EariliestEnd.HasValue || oldEntity.LatestEnd.HasValue)
            {
                mainRestriction.EndTimeLimitation = new EndTimeLimitation(oldEntity.EariliestEnd, oldEntity.LatestEnd);
            }
            if (oldEntity.EariliestStart.HasValue || oldEntity.LatestStart.HasValue)
            {
                mainRestriction.StartTimeLimitation = new StartTimeLimitation(oldEntity.EariliestStart, oldEntity.LatestStart);
            }
            if (oldEntity.MaxWorkTime.HasValue || oldEntity.MinWorkTime.HasValue)
            {
                TimeSpan max = TimeSpan.Zero;
                TimeSpan min = new TimeSpan(1).Subtract(new TimeSpan(1));
                if (oldEntity.MinWorkTime.HasValue)
                    min = (TimeSpan)oldEntity.MinWorkTime;
                if (oldEntity.MaxWorkTime.HasValue)
                    max = (TimeSpan)oldEntity.MaxWorkTime;
                mainRestriction.WorkTimeLimitation = new WorkTimeLimitation(min, max);
            }

            PreferenceDay personRestriction = new PreferenceDay(MappedObjectPair.Agent.GetPaired(oldEntity.Agent),
                                                      new DateOnly(oldEntity.PreferenceDate), mainRestriction);
            return personRestriction;
        }
    }
}

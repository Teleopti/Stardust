using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a dictionary of ISkillStaffPeriods, keyed by Period
    /// </summary>
    public interface ISkillStaffPeriodDictionary : IDictionary<DateTimePeriod, ISkillStaffPeriod>
    {
        /// <summary>
        /// Gets the skill open hours collection.
        /// </summary>
        /// <value>The skill open hours collection.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        ReadOnlyCollection<DateTimePeriod> SkillOpenHoursCollection { get; }

        /// <summary>
        /// Adds the specified skill staff period.
        /// </summary>
        /// <param name="skillStaffPeriod">The skill staff period.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-01-23
        /// </remarks>
        void Add(ISkillStaffPeriod skillStaffPeriod);

        /// <summary>
        /// Gets the skill of the skillstaff periods in this collection.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-12
        /// </remarks>
        IAggregateSkill Skill { get; }

        /// <summary>
        /// Tries the get resolution adjusted value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-05-13
        /// </remarks>
        bool TryGetResolutionAdjustedValue(DateTimePeriod key, out ISkillStaffPeriod value);

	    ILookup<HourSlot, ISkillStaffPeriod> ForLookup();
    }
}

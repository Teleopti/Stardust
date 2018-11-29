using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a dictionary of ISkillStaffPeriods, keyed by Period
    /// </summary>
    public interface ISkillStaffPeriodDictionary : IDictionary<DateTimePeriod, ISkillStaffPeriod>, IResourceCalculationPeriodDictionary
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
        /// Gets the skill of the skillstaff periods in this collection.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2009-02-12
        /// </remarks>
        IAggregateSkill Skill { get; }
    }
}

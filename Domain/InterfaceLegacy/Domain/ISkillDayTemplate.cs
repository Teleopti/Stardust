using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Used to represent skill data
    /// </summary>
    /// <remarks>
    /// Created by: micke
    /// Created date: 18.12.2007
    /// </remarks>
    public interface ISkillDayTemplate : IForecastDayTemplate, ICloneableEntity<ISkillDayTemplate>
    {
        /// <summary>
        /// Gets the skill data period template collection.
        /// </summary>
        /// <value>The skill data period template collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        ReadOnlyCollection<ITemplateSkillDataPeriod> TemplateSkillDataPeriodCollection { get; }

        /// <summary>
        /// Gets the skill resolution.
        /// </summary>
        /// <value>The skill resolution.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-10
        /// </remarks>
        int SkillResolution { get; }

        /// <summary>
        /// Sets the skill data period collection.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        void SetSkillDataPeriodCollection(IList<ITemplateSkillDataPeriod> periods);

        /// <summary>
        /// Splits the template skill data periods.
        /// Only template skill data periods owned by this skill day template will be splitted!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        void SplitTemplateSkillDataPeriods(IList<ITemplateSkillDataPeriod> list);

        /// <summary>
        /// Merges the template skill data periods.
        /// Only skill data periods owned by this skill day template will be merged!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        void MergeTemplateSkillDataPeriods(IList<ITemplateSkillDataPeriod> list);
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Used to represent multi site skill day template data
    /// </summary>
    /// <remarks>
    /// Created by: micke
    /// Created date: 18.12.2007
    /// </remarks>
    public interface IMultisiteDayTemplate : IForecastDayTemplate, ICloneableEntity<IMultisiteDayTemplate>
    {
        /// <summary>
        /// Gets the skill data period template collection.
        /// </summary>
        /// <value>The skill data period template collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        ReadOnlyCollection<ITemplateMultisitePeriod> TemplateMultisitePeriodCollection { get; }

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
        /// Sets the multisite period collection.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        void SetMultisitePeriodCollection(IList<ITemplateMultisitePeriod> periods);

        /// <summary>
        /// Splits the template multisite periods.
        /// Only template multisite periods owned by this multisite day template will be splitted!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        void SplitTemplateMultisitePeriods(IList<ITemplateMultisitePeriod> list);

        /// <summary>
        /// Merges the template multisite periods.
        /// Only multisite periods owned by this multisite day template will be merged!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        void MergeTemplateMultisitePeriods(IList<ITemplateMultisitePeriod> list);
    }
}
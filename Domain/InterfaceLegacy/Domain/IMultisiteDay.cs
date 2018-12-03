using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Multisite day information
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-23
    /// </remarks>
    public interface IMultisiteDay : IRestrictionChecker<IMultisiteDay>, ITemplateDay, IAggregateRoot, IChangeInfo, ICloneableEntity<IMultisiteDay>
    {
        /// <summary>
        /// Creates from template.
        /// </summary>
        /// <param name="multisiteDayDate">The multisite day date.</param>
        /// <param name="skill">The skill.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="multisiteDayTemplate">The multisite day template.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        void CreateFromTemplate(DateOnly multisiteDayDate, IMultisiteSkill skill, IScenario scenario, IMultisiteDayTemplate multisiteDayTemplate);

        /// <summary>
        /// Applies the template.
        /// </summary>
        /// <param name="templateDay">The template day.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        void ApplyTemplate(IMultisiteDayTemplate templateDay);

        /// <summary>
        /// Gets the skill data period collection.
        /// </summary>
        /// <value>The skill data period collection.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-30
        /// </remarks>
        ReadOnlyCollection<IMultisitePeriod> MultisitePeriodCollection { get; }

        /// <summary>
        /// Gets the multisite day date.
        /// </summary>
        /// <value>The multisite day date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        DateOnly MultisiteDayDate { get; }

        /// <summary>
        /// Gets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        IMultisiteSkill Skill { get; }

        /// <summary>
        /// Gets the scenario.
        /// </summary>
        /// <value>The scenario.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        IScenario Scenario { get; }

        /// <summary>
        /// Gets the child skill days.
        /// </summary>
        /// <value>The child skill days.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        IList<ISkillDay> ChildSkillDays { get; }

        /// <summary>
        /// Gets or sets the multisite skill day.
        /// </summary>
        /// <value>The multisite skill day.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        ISkillDay MultisiteSkillDay { get; set; }

        /// <summary>
        /// Sets the multisite period collection.
        /// </summary>
        /// <param name="periods">The periods.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-15
        /// </remarks>
        void SetMultisitePeriodCollection(IList<IMultisitePeriod> periods);

        /// <summary>
        /// Updates the name of the template.
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-02-14
        /// </remarks>
        void UpdateTemplateName();

        /// <summary>
        /// Splits the multisite periods.
        /// Only template multisite periods owned by this multisite day will be splitted!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        void SplitMultisitePeriods(IList<IMultisitePeriod> list);

        /// <summary>
        /// Merges the multisite periods.
        /// Only multisite periods owned by this multisite day will be merged!
        /// </summary>
        /// <param name="list">The list.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-15
        /// </remarks>
        void MergeMultisitePeriods(IList<IMultisitePeriod> list);

        /// <summary>
        /// Sets the child skill days.
        /// </summary>
        /// <param name="childSkillDays">The child skill days.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        void SetChildSkillDays(IEnumerable<ISkillDay> childSkillDays);

        /// <summary>
        /// Redistributes the childs.
        /// The skill day must already gone through CalculateStaff method!
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-29
        /// </remarks>
        void RedistributeChilds();

        /// <summary>
        /// Nones the entity clone.
        /// </summary>
        /// <param name="scenario">The scenario.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-12-22
        /// </remarks>
        IMultisiteDay NoneEntityClone(IScenario scenario);

		///<summary>
		/// Occurs when value changed.
		///</summary>
		event EventHandler<EventArgs> ValueChanged;
	}
}
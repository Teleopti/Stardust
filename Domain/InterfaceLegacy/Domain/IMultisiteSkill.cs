using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a multi site skill.
    /// </summary>
    public interface IMultisiteSkill : ISkill
    {
        /// <summary>
        /// Gets the child skills.
        /// </summary>
        /// <value>The child skills.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-18
        /// </remarks>
        ReadOnlyCollection<IChildSkill> ChildSkills { get; }

        /// <summary>
        /// Gets all multisite templates.
        /// </summary>
        /// <value>All multisite templates.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-22
        /// </remarks>
        IDictionary<int, IMultisiteDayTemplate> TemplateMultisiteWeekCollection { get; }

        /// <summary>
        /// Removes the child skill.
        /// </summary>
        /// <param name="childSkill">The child skill.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        void RemoveChildSkill(IChildSkill childSkill);

        /// <summary>
        /// Adds the child skill.
        /// </summary>
        /// <param name="childSkill">The child skill.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-25
        /// </remarks>
        void AddChildSkill(IChildSkill childSkill);
    }
}
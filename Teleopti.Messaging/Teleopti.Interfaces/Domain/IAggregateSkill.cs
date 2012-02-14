using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// To hold real skill references on a virtual skill
    /// for grouping in scheduler
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2009-01-19
    /// </remarks>
    public interface IAggregateSkill
    {
        /// <summary>
        /// Gets or sets the aggregate skills.
        /// </summary>
        /// <value>The aggregate skills.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-19
        /// </remarks>
        ReadOnlyCollection<ISkill> AggregateSkills { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is virtual.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is virtual; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-19
        /// </remarks>
        bool IsVirtual { get; set; }

        /// <summary>
        /// Adds the aggregate skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-19
        /// </remarks>
        void AddAggregateSkill(ISkill skill);

        /// <summary>
        /// Removes the aggregate skill.
        /// </summary>
        /// <param name="skill">The skill.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-19
        /// </remarks>
        void RemoveAggregateSkill(ISkill skill);

        /// <summary>
        /// Clears the aggregated skills.
        /// </summary>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-01-21
        /// </remarks>
        void ClearAggregateSkill();
    }
}

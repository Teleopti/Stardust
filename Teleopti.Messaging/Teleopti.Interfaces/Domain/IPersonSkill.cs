namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a <see cref="Skill"/> and <see cref="Percent"/> value pair. It gives information about 
    /// how effective, well trained a person on a given skill.
    /// </summary>
    /// <remarks>
    /// Created by: sumeda herath
    /// Created date: 2008-01-09
    /// </remarks>
    public interface IPersonSkill : IAggregateEntity, ICloneableEntity<IPersonSkill>
    {
        /// <summary>
        /// Gets or sets the skill.
        /// </summary>
        /// <value>The skill.</value>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        ISkill Skill { get; set; }

        /// <summary>
        /// Gets or sets the percent.
        /// </summary>
        /// <value>The percent.</value>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        Percent SkillPercentage { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="IPersonSkill"/> is active.
		/// </summary>
		/// <value>
		///   <c>true</c> if active; otherwise, <c>false</c>.
		/// </value>
		bool Active { get; set; }
    }
}

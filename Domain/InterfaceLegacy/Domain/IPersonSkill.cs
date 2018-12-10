namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IPersonSkillModify
	{
		/// <summary>
		/// Gets or sets the percent.
		/// </summary>
		/// <value>The percent.</value>
		Percent SkillPercentage { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="IPersonSkill"/> is active.
		/// </summary>
		/// <value>
		///   <c>true</c> if active; otherwise, <c>false</c>.
		/// </value>
		bool Active { get; set; }
	}

    /// <summary>
    /// Represents a <see cref="Skill"/> and <see cref="Percent"/> value pair. It gives information about 
    /// how effective, well trained a person on a given skill.
    /// </summary>
    public interface IPersonSkill : IAggregateEntity, ICloneableEntity<IPersonSkill>
    {
	    bool HasActivity(IActivity activity);
        /// <summary>
        /// Gets or sets the skill.
        /// </summary>
        /// <value>The skill.</value>
        ISkill Skill { get; }

        /// <summary>
        /// Gets or sets the percent.
        /// </summary>
        /// <value>The percent.</value>
        Percent SkillPercentage { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="IPersonSkill"/> is active.
		/// </summary>
		/// <value>
		///   <c>true</c> if active; otherwise, <c>false</c>.
		/// </value>
		bool Active { get; }
    }
}

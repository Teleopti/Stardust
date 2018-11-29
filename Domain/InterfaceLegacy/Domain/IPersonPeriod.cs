using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IPersonPeriodModifySkills
	{
		/// <summary>
		/// Adds the person skill.
		/// </summary>
		/// <param name="personSkill">The person skill.</param>
		/// <returns></returns>
		void AddPersonSkill(IPersonSkill personSkill);

		/// <summary>
		/// Deletes the person skill.
		/// </summary>
		/// <param name="personSkill">The person skill.</param>
		void DeletePersonSkill(IPersonSkill personSkill);

		/// <summary>
		/// Resets the person skill.
		/// </summary>
		void ResetPersonSkill();
	}

    /// <summary>
    /// Represents the concrete circumstances of a working period for a person (agent).
    /// Defines where, in which team, under what conditions the person WILL, and also 
    /// in what skills the person CAN work.  
    /// </summary>
    public interface IPersonPeriod : IAggregateEntity, ICloneableEntity<IPersonPeriod>
    {
        /// <summary>
        /// StartDate
        /// </summary>
        /// <remarks>
        /// Use the setter with caution, if possible, please use <see cref="IPerson.ChangePersonPeriodStartDate"/>.
        /// </remarks>
		DateOnly StartDate { get; set; }

        /// <summary>
        /// Gets the period
        /// </summary>
        DateOnlyPeriod Period { get; }

        /// <summary>
        /// Represent Person contract
        /// </summary>
        IPersonContract PersonContract { get; set; }

        /// <summary>
        /// Represent Team
        /// </summary>
        ITeam Team { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        string Note { get; set; }

        /// <summary>
        /// Gets or sets the person skill collection.
        /// </summary>
        /// <value>The person skill collection.</value>
        IEnumerable<IPersonSkill> PersonSkillCollection { get; }

	    IEnumerable<IPersonSkill> CascadingSkills();

        /// <summary>
        /// Gets or sets the rule set bag.
        /// </summary>
        /// <value>The rule set bag.</value>
        IRuleSetBag RuleSetBag { get; set; }

        /// <summary>
        /// Gets the period enddate
        /// </summary>
        /// <value>The end date.</value>
        DateOnly EndDate();

        /// <summary>
        /// Gets the person external log on collection.
        /// </summary>
        /// <value>The person external log on collection.</value>
        IEnumerable<IExternalLogOn> ExternalLogOnCollection { get; }

    	///<summary>
    	/// Gets or sets the budget group
    	///</summary>
    	IBudgetGroup BudgetGroup { get; set; }

		MaxSeatSkill MaxSeatSkill { get; }

	    void SetMaxSeatSkill(MaxSeatSkill maxSeatSkill);

				/// <summary>
				/// Gets the person non blend skill collection.
				/// </summary>
				/// <value>The person non blend skill collection.</value>
		IList<IPersonSkill> PersonNonBlendSkillCollection { get; }

        /// <summary>
        /// Adds the person non blend skill.
        /// </summary>
        /// <param name="personSkill">The person skill.</param>
        void AddPersonNonBlendSkill(IPersonSkill personSkill);
    }

	public interface IPersonPeriodModifyExternalLogon
	{
		void AddExternalLogOn(IExternalLogOn externalLogOn);
		void ResetExternalLogOn();
		void RemoveExternalLogOn(IExternalLogOn externalLogOn);
	}
}

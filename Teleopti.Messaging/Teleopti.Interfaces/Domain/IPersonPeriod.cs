using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents the concrete circumstances of a working period for a person (agent).
    /// Defines where, in which team, under what conditions the person WILL, and also 
    /// in what skills the person CAN work.  
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-01-09
    /// </remarks>
    public interface IPersonPeriod : IAggregateEntity, ICloneableEntity<IPersonPeriod>
    {
        /// <summary>
        /// StartDate
        /// </summary>
        /// /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-08
        /// </remarks>
        DateOnly StartDate { get; set; }

        /// <summary>
        /// Gets the period
        /// </summary>
        /// <remarks>
        /// Created by: cs
        /// Created date: 2008-03-06
        /// </remarks>
        DateOnlyPeriod Period { get; }

        /// <summary>
        /// Represent Person contract
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-01-09
        /// </remarks>
        IPersonContract PersonContract { get; set; }

        /// <summary>
        /// Represent Team
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-01-09
        /// </remarks>
        ITeam Team { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>The note.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-10-13
        /// </remarks>
        string Note { get; set; }

        /// <summary>
        /// Gets or sets the person skill collection.
        /// </summary>
        /// <value>The person skill collection.</value>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        IList<IPersonSkill> PersonSkillCollection { get; }

        /// <summary>
        /// Gets or sets the rule set bag.
        /// </summary>
        /// <value>The rule set bag.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        IRuleSetBag RuleSetBag { get; set; }

        /// <summary>
        /// Gets the period enddate
        /// </summary>
        /// <value>The end date.</value>
        DateOnly EndDate();

        /// <summary>
        /// Adds the person skill.
        /// </summary>
        /// <param name="personSkill">The person skill.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-01-09
        /// </remarks>
        void AddPersonSkill(IPersonSkill personSkill);

        /// <summary>
        /// Deletes the person skill.
        /// </summary>
        /// <param name="personSkill">The person skill.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-04-02
        /// </remarks>
        void DeletePersonSkill(IPersonSkill personSkill);

        /// <summary>
        /// Resets the person skill.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-08
        /// </remarks>
        void ResetPersonSkill();

        /// <summary>
        /// Gets the person external log on collection.
        /// </summary>
        /// <value>The person external log on collection.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-08-15
        /// </remarks>
        ReadOnlyCollection<IExternalLogOn> ExternalLogOnCollection { get; }

    	///<summary>
    	/// Gets or sets the budget group
    	///</summary>
    	IBudgetGroup BudgetGroup { get; set; }

    	/// <summary>
        /// Adds the external log on.
        /// </summary>
        /// <param name="externalLogOn">The external log on.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-08-15
        /// </remarks>
        void AddExternalLogOn(IExternalLogOn externalLogOn);
        /// <summary>
        /// Resets the external log on.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-08-15
        /// </remarks>
        void ResetExternalLogOn();

        /// <summary>
        /// Removes the external log on.
        /// </summary>
        /// <param name="externalLogOn">The external log on.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-18
        /// </remarks>
        void RemoveExternalLogOn(IExternalLogOn externalLogOn);

		/// <summary>
		/// Gets the person max seat skill collection.
		/// </summary>
		/// <value>The person max seat skill collection.</value>
		IList<IPersonSkill> PersonMaxSeatSkillCollection { get; }

		/// <summary>
		/// Adds the person max seat skill.
		/// </summary>
		/// <param name="personSkill">The person skill.</param>
		void AddPersonMaxSeatSkill(IPersonSkill personSkill);

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
}

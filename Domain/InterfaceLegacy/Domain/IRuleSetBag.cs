using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// A bag of workshiftrulesets
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-03-27
	/// </remarks>
	public interface IRuleSetBag : IAggregateRoot, ICloneableEntity<IRuleSetBag>
	{
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-03-27
		/// </remarks>
		Description Description { get; set; }

		/// <summary>
		/// Gets the rule set collection.
		/// </summary>
		/// <value>The rule set collection.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-03-27
		/// </remarks>
		ReadOnlyCollection<IWorkShiftRuleSet> RuleSetCollection { get; }

		/// <summary>
		/// Adds a rule set.
		/// </summary>
		/// <param name="workShiftRuleSet">The work shift rule set.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-03-27
		/// </remarks>
		void AddRuleSet(IWorkShiftRuleSet workShiftRuleSet);

		/// <summary>
		/// Removes the rule set.
		/// </summary>
		/// <param name="workShiftRuleSet">The work shift rule set.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-03-27
		/// </remarks>
		void RemoveRuleSet(IWorkShiftRuleSet workShiftRuleSet);

		/// <summary>
		/// Clears the rulesetcollection.
		/// </summary>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-04-28
		/// </remarks>
		void ClearRuleSetCollection();

		/// <summary>
		/// Gets a value indicating whether this instance is choosable.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is choosable; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-09-09
		/// </remarks>
		bool IsChoosable { get; }

		/// <summary>
		/// Shifts the categories in bag.
		/// </summary>
		/// <returns></returns>
		IList<IShiftCategory> ShiftCategoriesInBag();

		#region RK (and MS) - Put these outside of this entity!!

		/// <summary>
		/// Returns MinMax of WorkTimeLimitation, StartTimeLimitation and EndTimeLimitation.
		/// </summary>
		/// <param name="workShiftWorkTime"> </param>
		/// <param name="onDate">The on date.</param>
		/// <param name="restriction">The restriction.</param>
		/// <returns></returns>
		IWorkTimeMinMax MinMaxWorkTime(IWorkShiftWorkTime workShiftWorkTime, DateOnly onDate, IWorkTimeMinMaxRestriction restriction);

		#endregion
	}
}

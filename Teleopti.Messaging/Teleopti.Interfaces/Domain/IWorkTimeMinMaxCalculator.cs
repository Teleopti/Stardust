using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Interface for work time min/max calculator
	/// </summary>
	public interface IWorkTimeMinMaxCalculator
	{
		/// <summary>
		/// Calculate the min/max work times based on a rule set bag
		/// </summary>
		/// <param name="ruleSetBag">The sule set bag</param>
		/// <param name="date">The date</param>
		/// <returns></returns>
		IWorkTimeMinMax WorkTimeMinMax(IRuleSetBag ruleSetBag, DateOnly date);
	}
}
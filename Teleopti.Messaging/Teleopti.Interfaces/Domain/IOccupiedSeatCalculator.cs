using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Calculates and sets occupied seats on skillstaffperiod
	/// </summary>
	public interface IOccupiedSeatCalculator
	{
		/// <summary>
		/// Calculates the specified day.
		/// </summary>
		/// <param name="day">The day.</param>
		/// <param name="relevantProjections">The relevant projections.</param>
		/// <param name="relevantSkillStaffPeriods">The relevant skill staff periods.</param>
		void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods);
	}
}
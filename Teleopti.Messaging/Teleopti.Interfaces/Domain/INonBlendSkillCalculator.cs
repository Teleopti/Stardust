using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Calculates and sets resourse on skillstaffperiod for non blended skills
	/// </summary>
	public interface INonBlendSkillCalculator
	{
        /// <summary>
        /// Calculates the specified day.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="relevantProjections">The relevant projections.</param>
        /// <param name="relevantSkillStaffPeriods">The relevant skill staff periods.</param>
        /// <param name="addToEarlierResult">if set to <c>true</c> [add to earlier result].</param>
        void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections, ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, bool addToEarlierResult);
	}
}
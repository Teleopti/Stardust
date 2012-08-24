using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IRelativeDailyDifferencesByAllSkillsExtractor
	{
		/// <summary>
		/// Valueses the specified period.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		IList<double?> Values(DateOnlyPeriod period);
	}
}
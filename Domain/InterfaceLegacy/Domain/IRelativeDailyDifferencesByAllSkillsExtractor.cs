using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{

	/// <summary>
	/// Specific interface for this class
	/// </summary>
	public interface IRelativeDailyDifferencesByAllSkillsExtractor
	{
		/// <summary>
		/// Valueses the specified period.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IList<double?> Values(DateOnlyPeriod period);
	}
}
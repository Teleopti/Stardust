
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface ISwapAndModifyServiceNew
	{
		/// <summary>
		/// Swap modify service new
		/// </summary>
		/// <param name="person1">The person1.</param>
		/// <param name="person2">The person2.</param>
		/// <param name="dates">The dates.</param>
		/// <param name="lockedDates">The locked dates.</param>
		/// <param name="scheduleDictionary">The schedule dictionary.</param>
		/// <param name="newBusinessRuleCollection">The new business rule collection.</param>
		/// <param name="scheduleTagSetter">The schedule tag setter.</param>
		/// <param name="trackedCommandInfo">optional info for distinguishing where notifications come from </param>
		/// <returns></returns>
		IEnumerable<IBusinessRuleResponse> Swap(IPerson person1, IPerson person2, IList<DateOnly> dates, IList<DateOnly> lockedDates, IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRuleCollection, IScheduleTagSetter scheduleTagSetter, TrackedCommandInfo trackedCommandInfo = null);
	}
}

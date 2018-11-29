using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Swap raw functionality
	/// </summary>
	public interface ISwapRawService
	{
		/// <summary>
		/// Swap schedule days between two selections
		/// </summary>
		/// <param name="schedulePartModifyAndRollbackService"></param>
		/// <param name="selectionOne"></param>
		/// <param name="selectionTwo"></param>
		/// <param name="locks"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		void Swap(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, IList<IScheduleDay> selectionOne, IList<IScheduleDay> selectionTwo, IDictionary<IPerson, IList<DateOnly>> locks);
	}
}

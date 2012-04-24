using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Finding most suitable shift
	/// </summary>
	public interface IShiftLengthDecider
	{
		/// <summary>
		/// Filters the list.
		/// </summary>
		/// <param name="shiftList">The shift list.</param>
		/// <param name="workShiftMinMaxCalculator">The work shift min max calculator.</param>
		/// <param name="matrix">The matrix.</param>
		/// <returns></returns>
		IList<IShiftProjectionCache> FilterList(IList<IShiftProjectionCache> shiftList,
		                                        IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
		                                        IScheduleMatrixPro matrix);
	}
}
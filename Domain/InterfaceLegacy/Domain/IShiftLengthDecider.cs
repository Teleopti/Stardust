﻿using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <returns></returns>
		IList<IShiftProjectionCache> FilterList(IList<IShiftProjectionCache> shiftList,
		                                        IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
		                                        IScheduleMatrixPro matrix, 
		                                        ISchedulingOptions schedulingOptions);
	}
}
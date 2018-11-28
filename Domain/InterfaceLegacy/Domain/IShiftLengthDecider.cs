using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Finding most suitable shift
	/// </summary>
	public interface IShiftLengthDecider
	{
		// Filters the list.
		IList<ShiftProjectionCache> FilterList(IList<ShiftProjectionCache> shiftList,
		                                        IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
		                                        IScheduleMatrixPro matrix, 
		                                        SchedulingOptions schedulingOptions,
												OpenHoursSkillResult openHoursResult);
	}
}
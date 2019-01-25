using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios
{
	public class TakeShiftWithLatestShiftStartIfSameShiftValue : IEqualWorkShiftValueDecider
	{
		public ShiftProjectionCache Decide(ShiftProjectionCache cache1, ShiftProjectionCache cache2)
		{
			return cache1.WorkShiftStartTime() > cache2.WorkShiftStartTime() ?
				cache1 :
				cache2;
		}
	}
}
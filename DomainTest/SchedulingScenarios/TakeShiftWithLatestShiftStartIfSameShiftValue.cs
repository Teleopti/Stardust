using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public class TakeShiftWithLatestShiftStartIfSameShiftValue : IEqualWorkShiftValueDecider
	{
		public IShiftProjectionCache Decide(IShiftProjectionCache cache1, IShiftProjectionCache cache2)
		{
			return cache1.WorkShiftStartTime > cache2.WorkShiftStartTime ?
				cache1 :
				cache2;
		}
	}
}
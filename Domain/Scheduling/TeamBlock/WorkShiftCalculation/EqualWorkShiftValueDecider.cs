using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IEqualWorkShiftValueDecider
	{
		IShiftProjectionCache Decide(IShiftProjectionCache cache1, IShiftProjectionCache cache2);
	}
	
	public class EqualWorkShiftValueDecider : IEqualWorkShiftValueDecider
	{
		public IShiftProjectionCache Decide(IShiftProjectionCache cache1, IShiftProjectionCache cache2)
		{
			return cache1;
		}
	}
}
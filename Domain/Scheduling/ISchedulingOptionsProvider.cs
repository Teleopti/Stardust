using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ISchedulingOptionsProvider
	{
		SchedulingOptions Fetch();
	}
}
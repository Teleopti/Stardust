using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IGroupPersonBuilderForOptimizationFactory
	{
		IGroupPersonBuilderForOptimization Create(ISchedulingOptions schedulingOptions);
	}
}
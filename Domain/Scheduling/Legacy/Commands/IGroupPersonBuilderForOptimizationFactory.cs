using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IGroupPersonBuilderForOptimizationFactory
	{
		IGroupPersonBuilderForOptimization Create(ISchedulingOptions schedulingOptions);
	}
}
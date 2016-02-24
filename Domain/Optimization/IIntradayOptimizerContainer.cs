using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IIntradayOptimizerContainer
	{
		void Execute(IEnumerable<IIntradayOptimizer2> optimizers);
	}
}
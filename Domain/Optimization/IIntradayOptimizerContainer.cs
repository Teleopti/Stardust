using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicIntraday_45508)]
	public interface IIntradayOptimizerContainer
	{
		void Execute(IEnumerable<IIntradayOptimizer2> optimizers);
	}
}
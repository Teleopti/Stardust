using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationLimitsForAgentFactory
	{
		public OptimizationLimitsForAgent Create(IOptimizationPreferences optimizationPreferences, IEnumerable<ITeamBlockInfo> teamBlockInfos)
		{
			if (optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeams)
				return new OptimizationLimitsForAgent(null);

			var data = new Dictionary<IPerson, OptimizationLimits>();
			foreach (var teamBlockInfo in teamBlockInfos)
			{
				var agent = teamBlockInfo.TeamInfo.GroupMembers.First();
				if (!data.ContainsKey(agent))
				{
					var scheduleDayEquator = new ScheduleDayEquator(new EditableShiftMapper());
					var matrix = teamBlockInfo.MatrixesForGroupAndBlock().First();
					var originalStateContainer = new ScheduleMatrixOriginalStateContainer(matrix, scheduleDayEquator);
					IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider = new OptimizationOverLimitByRestrictionDecider(new RestrictionChecker(), optimizationPreferences, originalStateContainer, new DaysOffPreferences());
					data[agent] = new OptimizationLimits(optimizationOverLimitByRestrictionDecider);
				}
			}
			return new OptimizationLimitsForAgent(data);
		}	
	}
}
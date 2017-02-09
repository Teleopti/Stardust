using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationLimitsFactory
	{
		public OptimizationLimits Create(IOptimizationPreferences optimizationPreferences, ITeamBlockInfo teamBlockInfo)
		{
			IOptimizationOverLimitByRestrictionDecider optimizationOverLimitByRestrictionDecider;
			if (optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeams)
			{
				optimizationOverLimitByRestrictionDecider = new optimizationOverLimitByRestrictionDeciderAcceptEverything();
			}
			else
			{
				var scheduleDayEquator = new ScheduleDayEquator(new EditableShiftMapper());
				var matrix = teamBlockInfo.MatrixesForGroupAndBlock().First();
				var originalStateContainer = new ScheduleMatrixOriginalStateContainer(matrix, scheduleDayEquator);
				optimizationOverLimitByRestrictionDecider = new OptimizationOverLimitByRestrictionDecider(new RestrictionChecker(),
					optimizationPreferences, originalStateContainer, new DaysOffPreferences());
			}
			return new OptimizationLimits(optimizationOverLimitByRestrictionDecider);
		}

		private class optimizationOverLimitByRestrictionDeciderAcceptEverything : IOptimizationOverLimitByRestrictionDecider
		{
			public bool MoveMaxDaysOverLimit()
			{
				return false;
			}

			public OverLimitResults OverLimitsCounts(IScheduleMatrixPro matrix)
			{
				throw new System.NotImplementedException();
			}

			public bool HasOverLimitIncreased(OverLimitResults lastOverLimitCounts, IScheduleMatrixPro matrix)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
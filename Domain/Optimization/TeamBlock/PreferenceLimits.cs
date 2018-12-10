using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class PreferenceLimits
	{
		private OverLimitResults lastOverLimitCounts;
		private OptimizationLimits optimizationLimits;
		private IScheduleMatrixPro matrix;
		
		public void MeasureBefore(IOptimizationPreferences optimizationPreferences, ITeamBlockInfo teamBlockInfo, DateOnly date)
		{
			var matrixes = teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(date); 
			if (matrixes.Count() == 1)
			{
				matrix = matrixes.Single();
				optimizationLimits = new OptimizationLimits(new OptimizationOverLimitByRestrictionDecider(
					new RestrictionChecker(), optimizationPreferences,
					new ScheduleMatrixOriginalStateContainer(matrix, new ScheduleDayEquator(new EditableShiftMapper())), null));
				lastOverLimitCounts = optimizationLimits.OverLimitsCounts(matrix);
			}
		}

		public bool WithinLimit()
		{
			return optimizationLimits == null || !optimizationLimits.HasOverLimitExceeded(lastOverLimitCounts, matrix);
		}
	}
}
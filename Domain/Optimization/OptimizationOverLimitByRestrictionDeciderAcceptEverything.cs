using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationOverLimitByRestrictionDeciderAcceptEverything : IOptimizationOverLimitByRestrictionDecider
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
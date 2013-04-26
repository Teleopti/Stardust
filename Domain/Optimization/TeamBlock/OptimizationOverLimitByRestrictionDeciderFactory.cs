using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface IOptimizationOverLimitByRestrictionDeciderFactory
	{
		IOptimizationOverLimitByRestrictionDecider CreateOptimizationOverLimitByRestrictionDecider(IScheduleMatrixPro matrix,
			ICheckerRestriction restrictionChecker,
			IOptimizationPreferences optimizationPreferences,
			IScheduleMatrixOriginalStateContainer originalStateContainer);
	}
	public class OptimizationOverLimitByRestrictionDeciderFactory : IOptimizationOverLimitByRestrictionDeciderFactory
	{
		public IOptimizationOverLimitByRestrictionDecider CreateOptimizationOverLimitByRestrictionDecider(IScheduleMatrixPro matrix,
		                                                                                                  ICheckerRestriction
			                                                                                                  restrictionChecker,
		                                                                                                  IOptimizationPreferences
			                                                                                                  optimizationPreferences,
		                                                                                                  IScheduleMatrixOriginalStateContainer
			                                                                                                  originalStateContainer)
		{
			return new OptimizationOverLimitByRestrictionDecider(matrix, restrictionChecker, optimizationPreferences,
			                                                     originalStateContainer);
		}
	}
}
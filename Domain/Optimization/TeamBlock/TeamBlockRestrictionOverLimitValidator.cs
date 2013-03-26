using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockRestrictionOverLimitValidator
	{
		bool Validate(IList<IScheduleMatrixPro> allPersonMatrixList,
									  IOptimizationPreferences optimizationPreferences,
									  ISchedulingOptions schedulingOptions,
									  ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ICheckerRestriction restrictionChecker);
	}

	public class TeamBlockRestrictionOverLimitValidator : ITeamBlockRestrictionOverLimitValidator
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IScheduleMatrixOriginalStateContainerFactory _scheduleMatrixOriginalStateContainerFactory;
		private readonly IOptimizationOverLimitByRestrictionDeciderFactory _optimizationOverLimitByRestrictionDeciderFactory;

		public TeamBlockRestrictionOverLimitValidator(IScheduleDayEquator scheduleDayEquator,
		                                              ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
		                                              IScheduleMatrixOriginalStateContainerFactory scheduleMatrixOriginalStateContainerFactory,
		                                              IOptimizationOverLimitByRestrictionDeciderFactory optimizationOverLimitByRestrictionDeciderFactory)
		{
			_scheduleDayEquator = scheduleDayEquator;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_scheduleMatrixOriginalStateContainerFactory = scheduleMatrixOriginalStateContainerFactory;
			_optimizationOverLimitByRestrictionDeciderFactory = optimizationOverLimitByRestrictionDeciderFactory;
		}

		public bool Validate(IList<IScheduleMatrixPro> allPersonMatrixList,
		                     IOptimizationPreferences optimizationPreferences, 
		                     ISchedulingOptions schedulingOptions, 
		                     ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, ICheckerRestriction restrictionChecker)
		{
			foreach (var matrix in allPersonMatrixList)
			{
				var originalStateContainer = _scheduleMatrixOriginalStateContainerFactory.CreateScheduleMatrixOriginalStateContainer(matrix, _scheduleDayEquator);
				var optimizerOverLimitDecider = _optimizationOverLimitByRestrictionDeciderFactory.CreateOptimizationOverLimitByRestrictionDecider(matrix, restrictionChecker,
				                                                                                                                                  optimizationPreferences,
				                                                                                                                                  originalStateContainer);
				var moveMaxDaysOverLimit = optimizerOverLimitDecider.MoveMaxDaysOverLimit();
				if (moveMaxDaysOverLimit)
				{
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					return false;
				}

				var daysOverLimit = optimizerOverLimitDecider.OverLimit();
				if (daysOverLimit.Count > 0)
				{
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOptions);
					return false;
				}
			}
			return true;
		} 
	}
}
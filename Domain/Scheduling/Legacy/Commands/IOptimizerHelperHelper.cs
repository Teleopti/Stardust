using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IOptimizerHelperHelper
	{
		void LockDaysForDayOffOptimization(IList<IScheduleMatrixPro> matrixList, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod);
		void LockDaysForIntradayOptimization(IList<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod);

		IPeriodValueCalculator CreatePeriodValueCalculator(IAdvancedPreferences advancedPreferences,
			IScheduleResultDataExtractor dataExtractor);

		IScheduleResultDataExtractor CreateAllSkillsDataExtractor(
			IAdvancedPreferences advancedPreferences,
			DateOnlyPeriod selectedPeriod,
			ISchedulingResultStateHolder stateHolder);
	}
}
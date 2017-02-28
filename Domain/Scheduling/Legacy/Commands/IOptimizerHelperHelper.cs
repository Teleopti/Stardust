using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IOptimizerHelperHelper
	{
		void LockDaysForDayOffOptimization(IEnumerable<IScheduleMatrixPro> matrixList, IOptimizationPreferences optimizationPreferences, DateOnlyPeriod selectedPeriod);
		void LockDaysForIntradayOptimization(IEnumerable<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod);

		IPeriodValueCalculator CreatePeriodValueCalculator(IAdvancedPreferences advancedPreferences,
			IScheduleResultDataExtractor dataExtractor);

		IScheduleResultDataExtractor CreateAllSkillsDataExtractor(
			IAdvancedPreferences advancedPreferences,
			DateOnlyPeriod selectedPeriod,
			ISchedulingResultStateHolder stateHolder);

		IScheduleResultDataExtractor CreateTeamBlockAllSkillsDataExtractor(
			IAdvancedPreferences advancedPreferences,
			DateOnlyPeriod selectedPeriod,
			ISchedulingResultStateHolder stateHolder,
			IList<IScheduleMatrixPro> allScheduleMatrixPros);
	}
}
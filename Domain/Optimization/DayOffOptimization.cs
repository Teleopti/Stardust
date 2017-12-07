using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimization
	{
		private readonly TeamBlockDayOffOptimizer _teamBlockDayOffOptimizer;

		public DayOffOptimization(TeamBlockDayOffOptimizer teamBlockDayOffOptimizer)
		{
			_teamBlockDayOffOptimizer = teamBlockDayOffOptimizer;
		}
		
		public void Execute(IEnumerable<IScheduleMatrixPro> matrixList, DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences, SchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IBlockPreferenceProvider blockPreferenceProvider, ITeamInfoFactory teamInfoFactory,
			ISchedulingProgress backgroundWorker)
		{
			_teamBlockDayOffOptimizer.OptimizeDaysOff(matrixList,
				selectedPeriod,
				selectedPersons,
				optimizationPreferences,
				schedulingOptions,
				resourceCalculateDelayer,
				dayOffOptimizationPreferenceProvider,
				blockPreferenceProvider,
				teamInfoFactory,
				backgroundWorker);
		}
	}
}
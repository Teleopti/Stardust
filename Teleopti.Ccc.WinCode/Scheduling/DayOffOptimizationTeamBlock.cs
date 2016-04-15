using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class DayOffOptimizationTeamBlock : IDayOffOptimization
	{
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly ITeamBlockDayOffOptimizerService _teamBlockDayOffOptimizerService;

		public DayOffOptimizationTeamBlock(
			IResourceOptimizationHelper resourceOptimizationHelper,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			ITeamBlockDayOffOptimizerService teamBlockDayOffOptimizerService)
		{
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_teamBlockDayOffOptimizerService = teamBlockDayOffOptimizerService;
		}

		public void Execute(IList<IScheduleMatrixOriginalStateContainer> matrixOriginalStateContainerListForDayOffOptimization,
													DateOnlyPeriod selectedPeriod,
													ISchedulingProgress backgroundWorker,
													IOptimizationPreferences optimizerPreferences,
													IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var schedulingOptions = new SchedulingOptionsCreator().CreateSchedulingOptions(optimizerPreferences);
			var matrixListForDayOffOptimization = matrixOriginalStateContainerListForDayOffOptimization.Select(container => container.ScheduleMatrix).ToList();
			var selectedPersons = matrixListForDayOffOptimization.Select(matrixList => matrixList.Person).Distinct().ToList();

			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceOptimizationHelper, 1, schedulingOptions.ConsiderShortBreaks);
			_groupPersonBuilderWrapper.Reset();
			_groupPersonBuilderWrapper.SetSingleAgentTeam();
			var teamInfoFactory = new TeamInfoFactory(_groupPersonBuilderWrapper);

			_teamBlockDayOffOptimizerService.OptimizeDaysOff(matrixListForDayOffOptimization,
				selectedPeriod,
				selectedPersons,
				optimizerPreferences,
				schedulingOptions,
				resourceCalculateDelayer,
				dayOffOptimizationPreferenceProvider,
				teamInfoFactory,
				backgroundWorker);
		}
	}
}
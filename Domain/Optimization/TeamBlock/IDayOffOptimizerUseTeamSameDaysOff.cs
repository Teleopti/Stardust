using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamSameDayOff_44265)]
	public interface IDayOffOptimizerUseTeamSameDaysOff
	{
		IEnumerable<ITeamInfo> Execute(IPeriodValueCalculator periodValueCalculatorForAllSkills, IOptimizationPreferences optimizationPreferences, ISchedulePartModifyAndRollbackService rollbackService,
			IEnumerable<ITeamInfo> remainingInfoList, SchedulingOptions schedulingOptions, IEnumerable<IPerson> selectedPersons,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder,
			Action cancelAction,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, ISchedulingProgress schedulingProgress);
	}
}
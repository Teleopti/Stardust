using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	//remove me when toggle 37998 is set to true. And clean up unnecesssary params
	[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamBlockDayOffForIndividuals_37998)]
	public interface IDayOffOptimizationDesktop
	{
		void Execute(DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedAgents,
			ISchedulingProgress backgroundWorker, IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			GroupPageLight groupPageLight,
			Func<IWorkShiftFinderResultHolder> workShiftFinderResultHolder, 
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized);
	}
}
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopContextStateData
	{
		public DesktopContextStateData(ISchedulerStateHolder schedulerStateHolderFrom, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, IOptimizationCallback optimizationCallback)
		{
			SchedulerStateHolderFrom = schedulerStateHolderFrom;
			OptimizationPreferences = optimizationPreferences;
			DayOffOptimizationPreferenceProvider = dayOffOptimizationPreferenceProvider;
			OptimizationCallback = optimizationCallback;
		}

		public DesktopContextStateData(ISchedulerStateHolder schedulerStateHolderFrom, SchedulingOptions schedulingOptions, ISchedulingCallback schedulingCallback)
		{
			SchedulerStateHolderFrom = schedulerStateHolderFrom;
			SchedulingOptions = schedulingOptions;
			SchedulingCallback = schedulingCallback;
		}

		public ISchedulerStateHolder SchedulerStateHolderFrom { get; }
		public IOptimizationPreferences OptimizationPreferences { get; }
		public IDayOffOptimizationPreferenceProvider DayOffOptimizationPreferenceProvider { get; }
		public IOptimizationCallback OptimizationCallback { get; }
		public SchedulingOptions SchedulingOptions { get; }
		public ISchedulingCallback SchedulingCallback { get; }
	}
}
using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopContextState : IOptimizationPreferencesProvider, 
		ICurrentOptimizationCallback, 
		IBlockPreferenceProviderForPlanningPeriod,
		IDayOffOptimizationPreferenceProviderForPlanningPeriod,
		ISchedulingOptionsProvider, 
		ICurrentSchedulingCallback
	{
		private readonly DesktopContext _desktopContext;

		public DesktopContextState(DesktopContext desktopContext)
		{
			_desktopContext = desktopContext;
		}

		private DesktopContextStateData contextData()
		{
			return _desktopContext.CurrentContext();
		}

		public IDisposable SetForOptimization(ICommandIdentifier commandIdentifier, 
			ISchedulerStateHolder schedulerStateHolderFrom, 
			IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IOptimizationCallback optimizationCallback)
		{
			return _desktopContext.SetContextFor(commandIdentifier, new DesktopContextStateData(schedulerStateHolderFrom, optimizationPreferences, dayOffOptimizationPreferenceProvider, optimizationCallback));
		}
		
		public IDisposable SetForScheduling(ICommandIdentifier commandIdentifier, ISchedulerStateHolder schedulerStateHolderFrom, SchedulingOptions schedulingOptions, ISchedulingCallback schedulingCallback)
		{
			return _desktopContext.SetContextFor(commandIdentifier, new DesktopContextStateData(schedulerStateHolderFrom, schedulingOptions, schedulingCallback));
		}

		public IOptimizationPreferences Fetch()
		{
			return contextData().OptimizationPreferences;
		}
		
		public IBlockPreferenceProvider Fetch(AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			return contextData().IsOptimization ? 
				new FixedBlockPreferenceProvider(Fetch().Extra) : 
				new FixedBlockPreferenceProvider(((ISchedulingOptionsProvider)this).Fetch(null));
		}

		public IOptimizationCallback Current()
		{
			var ctxData = contextData();
			return ctxData == null ? new NoOptimizationCallback() : ctxData.OptimizationCallback;
		}

		IDayOffOptimizationPreferenceProvider IDayOffOptimizationPreferenceProviderForPlanningPeriod.Fetch(AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			return contextData().DayOffOptimizationPreferenceProvider;
		}
		
		public SchedulingOptions Fetch(IDayOffTemplate defaultDayOffTemplate)
		{
			return _desktopContext.CurrentContext().SchedulingOptions.Clone();
		}

		ISchedulingCallback ICurrentSchedulingCallback.Current()
		{
			return _desktopContext.CurrentContext().SchedulingCallback;
		}
	}
}
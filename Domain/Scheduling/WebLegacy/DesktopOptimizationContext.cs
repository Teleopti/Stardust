using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopOptimizationContext : IOptimizationPreferencesProvider, 
		ICurrentOptimizationCallback, 
		IBlockPreferenceProviderForPlanningPeriod,
		IDayOffOptimizationPreferenceProviderForPlanningPeriod
	{
		private readonly DesktopContext _desktopContext;

		public DesktopOptimizationContext(DesktopContext desktopContext)
		{
			_desktopContext = desktopContext;
		}

		private class desktopOptimizationContextData : IDesktopContextData
		{
			public desktopOptimizationContextData(ISchedulerStateHolder schedulerStateHolderFrom, IOptimizationPreferences optimizationPreferences, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider, IOptimizationCallback optimizationCallback)
			{
				SchedulerStateHolderFrom = schedulerStateHolderFrom;
				OptimizationPreferences = optimizationPreferences;
				DayOffOptimizationPreferenceProvider = dayOffOptimizationPreferenceProvider;
				OptimizationCallback = optimizationCallback;
			}

			public ISchedulerStateHolder SchedulerStateHolderFrom { get; }
			public IOptimizationPreferences OptimizationPreferences { get; }
			public IDayOffOptimizationPreferenceProvider DayOffOptimizationPreferenceProvider { get; }
			public IOptimizationCallback OptimizationCallback { get; }
		}

		private desktopOptimizationContextData contextData()
		{
			return (desktopOptimizationContextData) _desktopContext.CurrentContext();
		}

		public IDisposable Set(ICommandIdentifier commandIdentifier, 
			ISchedulerStateHolder schedulerStateHolderFrom, 
			IOptimizationPreferences optimizationPreferences,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IOptimizationCallback optimizationCallback)
		{
			return _desktopContext.SetContextFor(commandIdentifier, new desktopOptimizationContextData(schedulerStateHolderFrom, optimizationPreferences, dayOffOptimizationPreferenceProvider, optimizationCallback));
		}

		public IOptimizationPreferences Fetch()
		{
			return contextData().OptimizationPreferences;
		}
		
		public IBlockPreferenceProvider Fetch(PlanningGroup planningGroup)
		{
			return new FixedBlockPreferenceProvider(Fetch().Extra);
		}

		public IOptimizationCallback Current()
		{
			var ctxData = contextData();
			return ctxData == null ? new NoOptimizationCallback() : ctxData.OptimizationCallback;
		}

		IDayOffOptimizationPreferenceProvider IDayOffOptimizationPreferenceProviderForPlanningPeriod.Fetch(PlanningGroup planningGroup)
		{
			return contextData().DayOffOptimizationPreferenceProvider;
		}
	}
}
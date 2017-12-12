using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopOptimizationContext : IOptimizationPreferencesProvider, ICurrentIntradayOptimizationCallback, IBlockPreferenceProviderForPlanningPeriod
	{
		private readonly DesktopContext _desktopContext;

		public DesktopOptimizationContext(DesktopContext desktopContext)
		{
			_desktopContext = desktopContext;
		}

		private class desktopOptimizationContextData : IDesktopContextData
		{
			public desktopOptimizationContextData(ISchedulerStateHolder schedulerStateHolderFrom, IOptimizationPreferences optimizationPreferences, IIntradayOptimizationCallback intradayOptimizationCallback)
			{
				SchedulerStateHolderFrom = schedulerStateHolderFrom;
				OptimizationPreferences = optimizationPreferences;
				IntradayOptimizationCallback = intradayOptimizationCallback;
			}

			public ISchedulerStateHolder SchedulerStateHolderFrom { get; }
			public IOptimizationPreferences OptimizationPreferences { get; }
			public IIntradayOptimizationCallback IntradayOptimizationCallback { get; }
		}

		private desktopOptimizationContextData contextData()
		{
			return (desktopOptimizationContextData) _desktopContext.CurrentContext();
		}

		public IDisposable Set(ICommandIdentifier commandIdentifier, ISchedulerStateHolder schedulerStateHolderFrom, IOptimizationPreferences optimizationPreferences, IIntradayOptimizationCallback intradayOptimizationCallback)
		{
			return _desktopContext.SetContextFor(commandIdentifier, new desktopOptimizationContextData(schedulerStateHolderFrom, optimizationPreferences, intradayOptimizationCallback));
		}

		public IOptimizationPreferences Fetch()
		{
			return contextData().OptimizationPreferences;
		}
		
		public IBlockPreferenceProvider Fetch(Guid planningPeriodId)
		{
			return new FixedBlockPreferenceProvider(Fetch().Extra);
		}

		public IIntradayOptimizationCallback Current()
		{
			var ctxData = contextData();
			return ctxData == null ? new NoIntradayOptimizationCallback() : ctxData.IntradayOptimizationCallback;
		}
	}
}
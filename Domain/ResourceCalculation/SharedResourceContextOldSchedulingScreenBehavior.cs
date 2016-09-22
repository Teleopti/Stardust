using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SharedResourceContextOldSchedulingScreenBehavior : ISharedResourceContext
	{
		private readonly IResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public SharedResourceContextOldSchedulingScreenBehavior(IResourceCalculationContextFactory resourceCalculationContextFactory, Func<ISchedulerStateHolder> stateHolder)
		{
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_stateHolder = stateHolder;
		}

		public IDisposable MakeSureExists(DateOnlyPeriod period, bool forceNewContext)
		{
			var stateHolder = _stateHolder();
			IDisposable disposableContext = null;
			if (!ResourceCalculationContext.InContext) //TODO: this if probably never returns false... would be nice to get rid of it!
			{
				disposableContext = _resourceCalculationContextFactory.Create(stateHolder.Schedules, stateHolder.SchedulingResultState.Skills, period);
			}
			return new GenericDisposable(() =>
			{
				disposableContext?.Dispose();
			});
		}
	}
}
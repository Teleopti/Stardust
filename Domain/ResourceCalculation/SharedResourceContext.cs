using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SharedResourceContext : ISharedResourceContext
	{
		private readonly IResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly Func<ISchedulerStateHolder> _stateHolder;
		private IResourceCalculationDataContainerWithSingleOperation _sharedContext;

		public SharedResourceContext(IResourceCalculationContextFactory resourceCalculationContextFactory, Func<ISchedulerStateHolder> stateHolder)
		{
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_stateHolder = stateHolder;
		}

		public IDisposable MakeSureExists(DateOnlyPeriod period, bool forceNewContext)
		{
			if (_sharedContext == null || forceNewContext)
			{
				var stateHolder = _stateHolder();
				_resourceCalculationContextFactory.Create(stateHolder.Schedules, stateHolder.SchedulingResultState.Skills, period);
				_sharedContext = ResourceCalculationContext.Fetch();
			}
			new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => _sharedContext));

			return new GenericDisposable(() =>{});
		}
	}
}
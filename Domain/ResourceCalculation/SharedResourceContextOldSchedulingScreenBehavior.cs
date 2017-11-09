using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle("Put code directly on scheduling screeen instead", Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public class SharedResourceContextOldSchedulingScreenBehaviorNew : ISharedResourceContextOldSchedulingScreenBehavior
	{
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public SharedResourceContextOldSchedulingScreenBehaviorNew(CascadingResourceCalculationContextFactory resourceCalculationContextFactory, Func<ISchedulerStateHolder> stateHolder)
		{
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_stateHolder = stateHolder;
		}

		public IDisposable MakeSureExists(DateOnlyPeriod period)
		{
			var stateHolder = _stateHolder();
			var disposableContext = _resourceCalculationContextFactory.Create(stateHolder, false, period);
			return new GenericDisposable(() =>
			{
				disposableContext?.Dispose();
			});
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public interface ISharedResourceContextOldSchedulingScreenBehavior
	{
		IDisposable MakeSureExists(DateOnlyPeriod period);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public class SharedResourceContextOldSchedulingScreenBehavior : ISharedResourceContextOldSchedulingScreenBehavior
	{
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly Func<ISchedulerStateHolder> _stateHolder;

		public SharedResourceContextOldSchedulingScreenBehavior(CascadingResourceCalculationContextFactory resourceCalculationContextFactory, Func<ISchedulerStateHolder> stateHolder)
		{
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
			_stateHolder = stateHolder;
		}

		//TODO: remove me?
		public IDisposable MakeSureExists(DateOnlyPeriod period)
		{
			var stateHolder = _stateHolder();
			IDisposable disposableContext = null;
			if (!ResourceCalculationContext.InContext) //TODO: this if probably never returns false... would be nice to get rid of it! (just kept old behavior for now)
			{
				disposableContext = _resourceCalculationContextFactory.Create(stateHolder, false, period);
			}
			return new GenericDisposable(() =>
			{
				disposableContext?.Dispose();
			});
		}
	}
}
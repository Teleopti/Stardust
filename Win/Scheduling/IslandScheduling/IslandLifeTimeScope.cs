using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.IslandScheduling;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.IslandScheduling
{
	public class IslandLifeTimeScope : IDisposable
	{
		private readonly ILifetimeScope _componentContext;
		private bool _disposed;

		public IslandLifeTimeScope(IComponentContext componentContext)
		{
			_componentContext = componentContext.Resolve<ILifetimeScope>().BeginLifetimeScope();
		}

		public IScheduleDictionary ScheduleAsIsland(VirtualSkillGroupsCreatorResult skillGroups, SkillGroupIslandsAnalyzer.Island island, IOptimizerOriginalPreferences optimizerOriginalPreferences,
			IList<IScheduleDay> selectedScheduleDays, IOptimizationPreferences optimizationPreferences, ISchedulerStateHolder schedulingScreenStateHolder)
		{
			var islandScheduler = _componentContext.Resolve<IslandScheduler>();
			var result = islandScheduler.RunIsland(skillGroups, island, optimizerOriginalPreferences, selectedScheduleDays,
				optimizationPreferences, schedulingScreenStateHolder);

			return result;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);           
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_componentContext.Dispose();
				// Free any other managed objects here.
				//
			}

			// Free any unmanaged objects here.
			//
			_disposed = true;
		}
	}
}
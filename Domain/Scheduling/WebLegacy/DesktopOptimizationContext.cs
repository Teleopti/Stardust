using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopOptimizationContext : ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization, ICurrentIntradayOptimizationCallback
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

		//maybe this code will be shared later...
		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			var schedulerScheduleDictionary = contextData().SchedulerStateHolderFrom.Schedules;
			foreach (var diff in modifiedScheduleDictionary.DifferenceSinceSnapshot())
			{
				var modifiedAssignment = diff.CurrentItem as IPersonAssignment;
				if (modifiedAssignment != null)
				{
					var toScheduleDay = schedulerScheduleDictionary[modifiedAssignment.Person].ScheduledDay(modifiedAssignment.Date);
					var fromScheduleDay = modifiedScheduleDictionary[modifiedAssignment.Person].ScheduledDay(modifiedAssignment.Date);
					toScheduleDay.Replace(modifiedAssignment);
					schedulerScheduleDictionary.Modify(ScheduleModifier.Scheduler, toScheduleDay, NewBusinessRuleCollection.Minimum(),
						new DoNothingScheduleDayChangeCallBack(), new ScheduleTagSetter(fromScheduleDay.ScheduleTag()));
				}
			}
		}

		public IDisposable Set(ICommandIdentifier commandIdentifier, ISchedulerStateHolder schedulerStateHolderFrom, IOptimizationPreferences optimizationPreferences, IIntradayOptimizationCallback intradayOptimizationCallback)
		{
			return _desktopContext.SetContextFor(commandIdentifier, new desktopOptimizationContextData(schedulerStateHolderFrom, optimizationPreferences, intradayOptimizationCallback));
		}

		public IOptimizationPreferences Fetch()
		{
			return contextData().OptimizationPreferences;
		}

		public IEnumerable<IPerson> Agents(DateOnlyPeriod period)
		{
			return contextData().SchedulerStateHolderFrom.SchedulingResultState.PersonsInOrganization;
		}

		public IIntradayOptimizationCallback Current()
		{
			return contextData().IntradayOptimizationCallback;
		}
	}
}
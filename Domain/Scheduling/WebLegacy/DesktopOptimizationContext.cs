using System;
using System.Collections.Concurrent;
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
		private class desktopOptimizationContextData
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
		private readonly ConcurrentDictionary<Guid, desktopOptimizationContextData> _contextPerCommand = new ConcurrentDictionary<Guid, desktopOptimizationContextData>();

		public ISchedulerStateHolder SchedulerStateHolderFrom()
		{
			return _contextPerCommand[CommandScope.Current().CommandId].SchedulerStateHolderFrom;
		}

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			var schedulerScheduleDictionary = SchedulerStateHolderFrom().Schedules;
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
			_contextPerCommand[commandIdentifier.CommandId] = new desktopOptimizationContextData(schedulerStateHolderFrom, optimizationPreferences, intradayOptimizationCallback);
			return new GenericDisposable(() =>
			{
				desktopOptimizationContextData foo;
				_contextPerCommand.TryRemove(commandIdentifier.CommandId, out foo);
			});
		}

		public IOptimizationPreferences Fetch()
		{
			return _contextPerCommand[CommandScope.Current().CommandId].OptimizationPreferences;
		}

		public IEnumerable<IPerson> Agents(DateOnlyPeriod period)
		{
			return SchedulerStateHolderFrom().SchedulingResultState.PersonsInOrganization;
		}

		public IIntradayOptimizationCallback Current()
		{
			return _contextPerCommand[CommandScope.Current().CommandId].IntradayOptimizationCallback;
		}
	}
}
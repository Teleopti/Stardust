using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleState
	{
		public ScheduleState(IEnumerable<ScheduledActivity> schedules, bool cacheSchedules)
		{
			Schedules = schedules;
			CacheSchedules = cacheSchedules;
		}

		public IEnumerable<ScheduledActivity> Schedules { get; }
		public bool CacheSchedules { get; }
	}

	public interface IScheduleCacheStrategy
	{
		bool ValidateCached(AgentState state, DateTime now);
		IEnumerable<ScheduledActivity> FilterSchedules(IEnumerable<ScheduledActivity> all, DateTime now);
	}

	public class ThreeDays : IScheduleCacheStrategy
	{
		public bool ValidateCached(AgentState state, DateTime now)
		{
			if (state.Schedule == null)
				return false;
			if (now.Date != state.ReceivedTime.GetValueOrDefault().Date)
				return false;
			return true;
		}

		public IEnumerable<ScheduledActivity> FilterSchedules(IEnumerable<ScheduledActivity> all, DateTime now)
		{
			return all;
		}
	}

	public class Filtered : IScheduleCacheStrategy
	{
		public bool ValidateCached(AgentState state, DateTime now)
		{
			if (state.Schedule == null)
				return false;

			if (now.Date != state.ReceivedTime.GetValueOrDefault().Date)
				return false;

			var currentNow = ScheduleInfo.ActivityForTime(state.Schedule, now);
			var currentThen = ScheduleInfo.ActivityForTime(state.Schedule, state.ReceivedTime.Value);
			if (currentNow == null && currentThen != null)
				return false;
			if (currentNow != null && currentThen == null)
				return false;

			var activitiesBetween = ScheduleInfo.ActivitiesBetween(state.Schedule, state.ReceivedTime.Value, now);
			if (currentNow == null && currentThen == null && activitiesBetween.Any())
				return false;
			var startTimes = activitiesBetween.Select(x => x.StartDateTime).Skip(1);
			var endTimes = activitiesBetween.Select(x => x.EndDateTime);
			var allIsAdjecent = startTimes.All(x => endTimes.Contains(x));
			if (currentNow != null && currentThen != null && !allIsAdjecent)
				return false;

			return true;
		}

		public IEnumerable<ScheduledActivity> FilterSchedules(IEnumerable<ScheduledActivity> all, DateTime now)
		{
			var filtered = Enumerable.Empty<ScheduledActivity>();

			var current = ScheduleInfo.ActivityForTime(all, now);
			if (current != null)
				filtered = filtered.Append(current);
			else
			{
				filtered = filtered.Append(ScheduleInfo.PreviousActivity(all, now));
				filtered = filtered.Append(ScheduleInfo.NextActivity(all, now));
			}

			filtered = filtered.Concat(ScheduleInfo.ActivitiesInTimeWindow(all, now));
			filtered = filtered.Concat(ScheduleInfo.AllAdjecentTo(all, filtered));

			filtered = filtered.Where(x => x != null);

			var last = filtered
				.Select(x => x.EndDateTime)
				.DefaultIfEmpty()
				.Max();
			if (last != DateTime.MinValue)
			{
				filtered = filtered.Concat(ScheduleInfo.ActivitiesInTimeWindow(all, last));
				filtered = filtered.Concat(ScheduleInfo.AllAdjecentTo(all, filtered));
			}

			return filtered
				.Distinct()
				.OrderBy(l => l.StartDateTime)
				.ToArray();
		}
	}
}
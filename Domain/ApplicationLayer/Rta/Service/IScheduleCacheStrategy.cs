using System;
using System.Collections.Generic;

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
	
}
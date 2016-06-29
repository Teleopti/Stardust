using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleInfo
	{
		private readonly DateTime _currentTime;
		private readonly Lazy<IEnumerable<ScheduledActivity>> _schedule;
		private readonly Lazy<AgentState> _stored;
		private readonly Lazy<ScheduledActivity> _currentActivity;
		private readonly Lazy<ScheduledActivity> _nextActivityInShift;
		private readonly Lazy<ScheduledActivity> _previousActivity;
		private readonly Lazy<ScheduledActivity> _nextActivity;
		private readonly Lazy<DateTime> _currentShiftStartTime;
		private readonly Lazy<DateTime> _currentShiftEndTime;
		private readonly Lazy<DateTime> _shiftStartTimeForPreviousActivity;
		private readonly Lazy<DateTime> _shiftEndTimeForPreviousActivity;
		private readonly Lazy<DateOnly?> _belongsToDate;
		private readonly Lazy<int> _timeWindowCheckSum;
		private readonly Lazy<IEnumerable<ScheduledActivity>> _timeWindowActivities;

		public ScheduleInfo(
			Lazy<IEnumerable<ScheduledActivity>> schedule,
			Lazy<AgentState> stored,
			DateTime currentTime
			)
		{
			_currentTime = currentTime;
			_stored = stored;
			_schedule = schedule;
			_currentActivity = new Lazy<ScheduledActivity>(() => activityForTime(currentTime));
			_nextActivityInShift = new Lazy<ScheduledActivity>(nextAdjecentActivityToCurrent);
			_currentShiftStartTime = new Lazy<DateTime>(() => startTimeOfShift(_currentActivity.Value));
			_currentShiftEndTime = new Lazy<DateTime>(() => endTimeOfShift(_currentActivity.Value));
			_previousActivity = new Lazy<ScheduledActivity>(() => (from l in _schedule.Value where l.EndDateTime <= currentTime select l).LastOrDefault());
			_nextActivity = new Lazy<ScheduledActivity>(() => (from l in _schedule.Value where l.StartDateTime <= currentTime select l).FirstOrDefault());
			_shiftStartTimeForPreviousActivity = new Lazy<DateTime>(() => startTimeOfShift(_previousActivity.Value));
			_shiftEndTimeForPreviousActivity = new Lazy<DateTime>(() => endTimeOfShift(_previousActivity.Value));
			_belongsToDate = new Lazy<DateOnly?>(() =>
			{
				var activity = CurrentActivity() ?? activityNear(_currentTime);
				if (activity != null)
					return activity.BelongsToDate;
				return null;
			});
			_timeWindowCheckSum = new Lazy<int>(() => ActivitiesInTimeWindow().CheckSum());
			_timeWindowActivities = new Lazy<IEnumerable<ScheduledActivity>>(() =>
			{
				var timeWindowStart = _currentTime.AddHours(-1);
				var timeWindowEnd = _currentTime.AddHours(3);
				return from a in _schedule.Value
					   where a.EndDateTime > timeWindowStart
					   where a.StartDateTime < timeWindowEnd
					   select a;
			});
		}

		public int TimeWindowCheckSum()
		{
			return _timeWindowCheckSum.Value;
		}

		public IEnumerable<ScheduledActivity> ActivitiesInTimeWindow()
		{
			return _timeWindowActivities.Value;
		}

		public bool ShiftStarted()
		{
			return _stored.Value?.ActivityId == null &&
				   CurrentActivity() != null;
		}

		public bool ShiftEnded()
		{
			return _stored.Value?.ActivityId != null &&
				   CurrentActivity() == null &&
				   PreviousActivity() != null;
		}

		public ScheduledActivity CurrentActivity()
		{
			return _currentActivity.Value;
		}

		public Guid? CurrentActivityId()
		{
			return _currentActivity.Value == null ? (Guid?)null : _currentActivity.Value.PayloadId;
		}

		public string CurrentActivityName()
		{
			return _currentActivity.Value == null ? null : _currentActivity.Value.Name;
		}

		public ScheduledActivity PreviousActivity()
		{
			return _previousActivity.Value;
		}

		public Guid? PreviousActivityId()
		{
			return _previousActivity.Value == null ? (Guid?)null : _previousActivity.Value.PayloadId;
		}

		public ScheduledActivity NextActivity()
		{
			return _nextActivity.Value;
		}

		public ScheduledActivity NextActivityInShift()
		{
			return _nextActivityInShift.Value;
		}

		public Guid? NextActivityId()
		{
			return _nextActivityInShift.Value == null ? (Guid?)null : _nextActivityInShift.Value.PayloadId;
		}

		public DateTime? NextActivityStartTime()
		{
			if (_nextActivityInShift.Value != null)
				return _nextActivityInShift.Value.StartDateTime;
			if (_currentActivity.Value != null)
				return _currentActivity.Value.EndDateTime;
			return null;
		}

		public string NextActivityName()
		{
			return _nextActivityInShift.Value != null ? _nextActivityInShift.Value.Name : null;
		}

		public DateTime CurrentShiftStartTime { get { return _currentShiftStartTime.Value; } }
		public DateTime CurrentShiftEndTime { get { return _currentShiftEndTime.Value; } }

		public DateTime ShiftStartTimeForPreviousActivity { get { return _shiftStartTimeForPreviousActivity.Value; } }
		public DateTime ShiftEndTimeForPreviousActivity { get { return _shiftEndTimeForPreviousActivity.Value; } }

		public DateOnly? BelongsToDate { get { return _belongsToDate.Value; } }

		private DateTime startTimeOfShift(ScheduledActivity activity)
		{
			if (activity == null)
				return DateTime.MinValue;
			return activitiesThisShift(activity).Select(x => x.StartDateTime).Min();
		}

		private DateTime endTimeOfShift(ScheduledActivity activity)
		{
			if (activity == null)
				return DateTime.MinValue;
			return activitiesThisShift(activity).Select(x => x.EndDateTime).Max();
		}

		private IEnumerable<ScheduledActivity> activitiesThisShift(ScheduledActivity activity)
		{
			return from l in _schedule.Value
				where l.BelongsToDate == activity.BelongsToDate
				select l;
		}

		private ScheduledActivity activityForTime(DateTime time)
		{
			return ActivityForTime(_schedule.Value, time);
		}

		private ScheduledActivity nextAdjecentActivityToCurrent()
		{
			var nextActivity = (from l in _schedule.Value where l.StartDateTime > _currentTime select l).FirstOrDefault();
			if (nextActivity == null)
				return null;
			if (_currentActivity.Value == null)
				return nextActivity;
			if (nextActivity.StartDateTime == _currentActivity.Value.EndDateTime)
				return nextActivity;
			return null;
		}

		private ScheduledActivity activityNear(DateTime time)
		{
			return (
				from l in _schedule.Value
				let ended = l.EndDateTime >= _currentTime.AddHours(-1) && l.StartDateTime < time
				let starting = l.StartDateTime <= _currentTime.AddHours(1) && l.EndDateTime > time
				where ended || starting
				select l
				).FirstOrDefault();
		}


		public static ScheduledActivity ActivityForTime(IEnumerable<ScheduledActivity> schedule, DateTime time)
		{
			return schedule.FirstOrDefault(l => time >= l.StartDateTime && time < l.EndDateTime);
		}

		public static ScheduledActivity PreviousActivity(IEnumerable<ScheduledActivity> schedule, DateTime time)
		{
			return schedule.LastOrDefault(l => l.EndDateTime <= time);
		}

		public static ScheduledActivity NextActivity(IEnumerable<ScheduledActivity> schedule, DateTime time)
		{
			return schedule.FirstOrDefault(l => l.StartDateTime > time);
		}

	}
}
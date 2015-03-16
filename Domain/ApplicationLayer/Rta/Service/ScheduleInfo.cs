using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleInfo
	{
		private readonly DateTime _currentTime;
		private readonly Lazy<IEnumerable<ScheduleLayer>> _scheduleLayers;
		private readonly Lazy<ScheduleLayer> _currentActivity;
		private readonly Lazy<ScheduleLayer> _nextActivityInShift;
		private readonly Lazy<ScheduleLayer> _previousActivity;
		private readonly Lazy<DateTime> _currentShiftStartTime;
		private readonly Lazy<DateTime> _currentShiftEndTime;
		private readonly Lazy<DateTime> _shiftStartTimeForPreviousActivity;
		private readonly Lazy<DateTime> _shiftEndTimeForPreviousActivity;
		private readonly Lazy<DateOnly?> _belongsToDate;

		public ScheduleInfo(IDatabaseReader databaseReader, Guid personId, DateTime currentTime)
		{
			_currentTime = currentTime;
			_scheduleLayers = new Lazy<IEnumerable<ScheduleLayer>>(() => databaseReader.GetCurrentSchedule(personId));
			_currentActivity = new Lazy<ScheduleLayer>(() => activityForTime(currentTime));
			_nextActivityInShift = new Lazy<ScheduleLayer>(nextAdjecentActivityToCurrent);
			_currentShiftStartTime = new Lazy<DateTime>(() => startTimeOfShift(_currentActivity.Value));
			_currentShiftEndTime = new Lazy<DateTime>(() => endTimeOfShift(_currentActivity.Value));
			_previousActivity = new Lazy<ScheduleLayer>(() => (from l in _scheduleLayers.Value where l.EndDateTime <= currentTime select l).LastOrDefault());
			_shiftStartTimeForPreviousActivity = new Lazy<DateTime>(() => startTimeOfShift(_previousActivity.Value));
			_shiftEndTimeForPreviousActivity = new Lazy<DateTime>(() => endTimeOfShift(_previousActivity.Value));
			_belongsToDate = new Lazy<DateOnly?>(() =>
			{
				var activity = CurrentActivity() ?? activityNear(_currentTime);
				if (activity != null)
					return activity.BelongsToDate;
				return null;
			});
		}

		public ScheduleLayer CurrentActivity()
		{
			return _currentActivity.Value;
		}

		public ScheduleLayer PreviousActivity()
		{
			return _previousActivity.Value;
		}

		public ScheduleLayer NextActivityInShift()
		{
			return _nextActivityInShift.Value;
		}

		public Guid? CurrentActivityId()
		{
			return _currentActivity.Value == null ? (Guid?)null : _currentActivity.Value.PayloadId;
		}

		public string CurrentActivityName()
		{
			return _currentActivity.Value == null ? null : _currentActivity.Value.Name;
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

		private DateTime startTimeOfShift(ScheduleLayer activity)
		{
			if (activity == null)
				return DateTime.MinValue;
			return activitiesThisShift(activity).Select(x => x.StartDateTime).Min();
		}

		private DateTime endTimeOfShift(ScheduleLayer activity)
		{
			if (activity == null)
				return DateTime.MinValue;
			return activitiesThisShift(activity).Select(x => x.EndDateTime).Max();
		}

		private IEnumerable<ScheduleLayer> activitiesThisShift(ScheduleLayer activity)
		{
			return from l in _scheduleLayers.Value
				where l.BelongsToDate == activity.BelongsToDate
				select l;
		}

		private ScheduleLayer activityForTime(DateTime time)
		{
			return _scheduleLayers.Value.FirstOrDefault(l => time >= l.StartDateTime && time < l.EndDateTime);
		}

		private ScheduleLayer nextAdjecentActivityToCurrent()
		{
			var nextActivity = (from l in _scheduleLayers.Value where l.StartDateTime > _currentTime select l).FirstOrDefault();
			if (nextActivity == null)
				return null;
			if (_currentActivity.Value == null)
				return nextActivity;
			if (nextActivity.StartDateTime == _currentActivity.Value.EndDateTime)
				return nextActivity;
			return null;
		}

		private ScheduleLayer activityNear(DateTime time)
		{
			return (
				from l in _scheduleLayers.Value
				let ended = l.EndDateTime >= _currentTime.AddHours(-1) && l.StartDateTime < time
				let starting = l.StartDateTime <= _currentTime.AddHours(1) && l.EndDateTime > time
				where ended || starting
				select l
				).FirstOrDefault();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class ScheduleInfo
	{
		private readonly Context _context;
		private readonly Lazy<IEnumerable<ScheduledActivity>> _schedule;
		private readonly Lazy<ScheduledActivity> _currentActivity;
		private readonly Lazy<ScheduledActivity> _nextActivity;
		private readonly Lazy<ScheduledActivity> _previousActivity;
		private readonly Lazy<DateTime> _currentShiftStartTime;
		private readonly Lazy<DateTime> _currentShiftEndTime;
		private readonly Lazy<DateTime> _shiftStartTimeForPreviousActivity;
		private readonly Lazy<DateTime> _shiftEndTimeForPreviousActivity;
		private readonly Lazy<DateOnly?> _belongsToDate;
		private readonly Lazy<int> _timeWindowCheckSum;
		private readonly Lazy<IEnumerable<ScheduledActivity>> _timeWindowActivities;
		
		public ScheduleInfo(Context context, Lazy<IEnumerable<ScheduledActivity>> schedule)
		{
			_context = context;
			_schedule = schedule;
			_currentActivity = new Lazy<ScheduledActivity>(() => activityForTime(context.CurrentTime));
			_nextActivity = new Lazy<ScheduledActivity>(nextActivity);
			_currentShiftStartTime = new Lazy<DateTime>(() => startTimeOfShift(_currentActivity.Value));
			_currentShiftEndTime = new Lazy<DateTime>(() => endTimeOfShift(_currentActivity.Value));
			_previousActivity = new Lazy<ScheduledActivity>(() => (from l in _schedule.Value where l.EndDateTime <= context.CurrentTime select l).LastOrDefault());
			_shiftStartTimeForPreviousActivity = new Lazy<DateTime>(() => startTimeOfShift(_previousActivity.Value));
			_shiftEndTimeForPreviousActivity = new Lazy<DateTime>(() => endTimeOfShift(_previousActivity.Value));
			_belongsToDate = new Lazy<DateOnly?>(() =>
			{
				var activity = CurrentActivity() ?? activityNear(context.CurrentTime);
				return activity?.BelongsToDate;
			});
			_timeWindowActivities = new Lazy<IEnumerable<ScheduledActivity>>(() => activitiesBetween(timeWindowStart(), timeWindowEnd()));
			_timeWindowCheckSum = new Lazy<int>(() => _timeWindowActivities.Value.CheckSum());
		}

		public bool ActivityChanged()
		{
			return _context.Stored?.ActivityId != _context.Schedule.CurrentActivityId();
		}

		public bool ShiftStarted()
		{
			return _context.Stored?.ActivityId == null &&
				   CurrentActivity() != null;
		}

		public bool ShiftEnded()
		{
			return _context.Stored?.ActivityId != null &&
				   CurrentActivity() == null &&
				   PreviousActivity() != null;
		}

		public int? TimeWindowCheckSum()
		{
			if (_timeWindowCheckSum.Value == 0)
				return null;
			return _timeWindowCheckSum.Value;
		}

		public IEnumerable<ScheduledActivity> ActivitiesInTimeWindow()
		{
			return _timeWindowActivities.Value;
		}

		public ScheduledActivity CurrentActivity()
		{
			return _currentActivity.Value;
		}

		public Guid? CurrentActivityId()
		{
			return _currentActivity.Value?.PayloadId;
		}

		public string CurrentActivityName()
		{
			return _currentActivity.Value?.Name;
		}

		public ScheduledActivity PreviousActivity()
		{
			return _previousActivity.Value;
		}

		public Guid? PreviousActivityId()
		{
			return _previousActivity.Value?.PayloadId;
		}
		
		public ScheduledActivity NextActivity()
		{
			return _nextActivity.Value;
		}

		public Guid? NextActivityId()
		{
			return _nextActivity.Value?.PayloadId;
		}

		public DateTime? NextActivityStartTime()
		{
			return _nextActivity.Value?.StartDateTime ?? _currentActivity.Value?.EndDateTime;
		}

		public DateTime? ActivityStartTime()
		{
			var currentActivity = _context.Schedule.CurrentActivity();
			if (currentActivity == null)
				return null;
			var previousStateTime = _context.Stored.ReceivedTime();
			var activityStartedInThePast = currentActivity.StartDateTime < previousStateTime;
			return activityStartedInThePast
				? previousStateTime
				: currentActivity.StartDateTime;
		}

		public string NextActivityName()
		{
			return _nextActivity.Value?.Name;
		}

		public DateTime CurrentShiftStartTime => _currentShiftStartTime.Value;
		public DateTime CurrentShiftEndTime => _currentShiftEndTime.Value;

		public DateTime ShiftStartTimeForPreviousActivity => _shiftStartTimeForPreviousActivity.Value;
		public DateTime ShiftEndTimeForPreviousActivity => _shiftEndTimeForPreviousActivity.Value;

		public DateOnly? BelongsToDate => _belongsToDate.Value;




		private DateTime startTimeOfShift(ScheduledActivity activity)
		{
			return activity == null ? DateTime.MinValue : activitiesThisShift(activity).Select(x => x.StartDateTime).Min();
		}

		private DateTime endTimeOfShift(ScheduledActivity activity)
		{
			return activity == null ? DateTime.MinValue : activitiesThisShift(activity).Select(x => x.EndDateTime).Max();
		}

		private IEnumerable<ScheduledActivity> activitiesThisShift(ScheduledActivity activity)
		{
			return from l in _schedule.Value
				where l.BelongsToDate == activity.BelongsToDate
				select l;
		}

		private ScheduledActivity activityForTime(DateTime time)
		{
			return _schedule.Value.FirstOrDefault(l => time >= l.StartDateTime && time < l.EndDateTime);
		}

		private ScheduledActivity nextActivity()
		{
			var nextActivity = (from l in _schedule.Value where l.StartDateTime > _context.CurrentTime select l).FirstOrDefault();
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
				let ended = l.EndDateTime >= _context.CurrentTime.AddHours(-1) && l.StartDateTime < time
				let starting = l.StartDateTime <= _context.CurrentTime.AddHours(1) && l.EndDateTime > time
				where ended || starting
				select l
				).FirstOrDefault();
		}
		
		private readonly TimeSpan timeWindowFuture = TimeSpan.FromHours(3);
		private readonly TimeSpan timeWindowPast = TimeSpan.FromHours(-1);

		private DateTime timeWindowStart()
		{
			return _context.CurrentTime.Add(timeWindowPast);
		}

		private DateTime timeWindowEnd()
		{
			return _context.CurrentTime.Add(timeWindowFuture);
		}

		private IEnumerable<ScheduledActivity> activitiesBetween(DateTime start, DateTime end)
		{
			return from a in _schedule.Value
				   where a.EndDateTime > start
				   where a.StartDateTime <= end
				   select a;
		}

		public DateTime? NextCheck()
		{
			var activityEnteringTimeWindow = _schedule.Value.FirstOrDefault(x => x.StartDateTime >= timeWindowEnd());
			var activityEntersTimeWindowAt = activityEnteringTimeWindow?.StartDateTime.Subtract(timeWindowFuture);
			return new[]
			{
				_currentActivity.Value?.EndDateTime,
				_nextActivity.Value?.StartDateTime,
				activityEntersTimeWindowAt
			}.Min();
		}
	}
}
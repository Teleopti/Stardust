using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class StateInfo
	{
		private readonly ExternalUserStateInputModel _input;
		private readonly Lazy<IEnumerable<ScheduleLayer>> _scheduleLayers;
		private readonly Lazy<ScheduleLayer> _activityForPreviousState;
		private readonly Lazy<ScheduleLayer> _currentActivity;
		private readonly Lazy<ScheduleLayer> _nextActivityInShift;
		private readonly Lazy<IActualAgentState> _previousState;
		private readonly Lazy<DateTime> _currentShiftStartTime;
		private readonly Lazy<DateTime> _currentShiftEndTime;
		private readonly Lazy<DateTime> _shiftStartTimeForPreviousState;
		private readonly Lazy<DateTime> _shiftEndTimeForPreviousState;
		private readonly Lazy<bool> _inAdherenceWithNewActivity;
		private readonly Lazy<IActualAgentState> _newState;
		private readonly Lazy<bool> _inAdherence;
		private readonly Lazy<bool> _inAdherenceWithPreviousActivity;

		private readonly IActualAgentAssembler _actualAgentStateAssembler;
		private Guid? _platformTypeId;
		private PersonWithBusinessUnit _person;
		private readonly DateTime _currentTime;

		public StateInfo(
			IDatabaseReader databaseReader,
			IActualAgentAssembler actualAgentStateAssembler,
			PersonWithBusinessUnit person,
			ExternalUserStateInputModel input,
			DateTime currentTime)
		{

			_input = input;
			_person = person;
			_actualAgentStateAssembler = actualAgentStateAssembler;
			_currentTime = currentTime;

			_newState = new Lazy<IActualAgentState>(() => actualAgentStateAssembler.GetAgentState(
				input,
				person,
				CurrentActivity,
				NextActivityInShift,
				PreviousState,
				currentTime));

			_previousState = new Lazy<IActualAgentState>(() => databaseReader.GetCurrentActualAgentState(person.PersonId) ??
															   new ActualAgentState
															   {
																   PersonId = person.PersonId,
																   StateId = Guid.NewGuid(),
															   });
			_scheduleLayers = new Lazy<IEnumerable<ScheduleLayer>>(() => databaseReader.GetCurrentSchedule(person.PersonId));
			_activityForPreviousState = new Lazy<ScheduleLayer>(() => activityForTime(PreviousState.ReceivedTime));
			_currentActivity = new Lazy<ScheduleLayer>(() => activityForTime(currentTime));
			_nextActivityInShift = new Lazy<ScheduleLayer>(nextAdjecentActivityToCurrent);
			_currentShiftStartTime = new Lazy<DateTime>(() => startTimeOfShift(CurrentActivity));
			_currentShiftEndTime = new Lazy<DateTime>(() => endTimeOfShift(CurrentActivity));
			_shiftStartTimeForPreviousState = new Lazy<DateTime>(() => startTimeOfShift(ActivityForPreviousState));
			_shiftEndTimeForPreviousState = new Lazy<DateTime>(() => endTimeOfShift(ActivityForPreviousState));

			_inAdherence = new Lazy<bool>(() => AdherenceFor(NewState));
			_inAdherenceWithNewActivity = new Lazy<bool>(() =>
			{
				if (CurrentActivity == ActivityForPreviousState)
					return true;
				return AdherenceFor(PreviousState.StateCode, NewState.ScheduledId);
			});
			_inAdherenceWithPreviousActivity = new Lazy<bool>(() =>
			{
				var previousActivity = (from l in ScheduleLayers where l.EndDateTime < currentTime select l).LastOrDefault();
				return AdherenceFor(_input.StateCode, previousActivity);
			});

		}

		public IEnumerable<ScheduleLayer> ScheduleLayers { get { return _scheduleLayers.Value; } }
		public IActualAgentState PreviousState { get { return _previousState.Value; } }
		public IActualAgentState NewState { get { return _newState.Value; } }

		public bool IsScheduled { get { return NewState.ScheduledId != Guid.Empty; } }
		public bool WasScheduled { get { return PreviousState.ScheduledId != Guid.Empty; }}

		public ScheduleLayer CurrentActivity { get { return _currentActivity.Value; } }
		public ScheduleLayer NextActivityInShift { get { return _nextActivityInShift.Value; } }
		public DateTime CurrentShiftStartTime { get { return _currentShiftStartTime.Value; } }
		public DateTime CurrentShiftEndTime { get { return _currentShiftEndTime.Value; } }

		public ScheduleLayer ActivityForPreviousState { get { return _activityForPreviousState.Value; } }
		public DateTime ShiftStartTimeForPreviousState { get { return _shiftStartTimeForPreviousState.Value; } }
		public DateTime ShiftEndTimeForPreviousState { get { return _shiftEndTimeForPreviousState.Value; } }

		public bool InAdherence { get { return _inAdherence.Value; } }
		public bool InAdherenceWithNewActivity { get { return _inAdherenceWithNewActivity.Value; } }
		public bool InAdherenceWithPreviousActivity { get { return _inAdherenceWithPreviousActivity.Value; } }

		public bool Send
		{
			get
			{
				return !NewState.ScheduledId.Equals(PreviousState.ScheduledId) ||
					   !NewState.ScheduledNextId.Equals(PreviousState.ScheduledNextId) ||
					   !NewState.AlarmId.Equals(PreviousState.AlarmId) ||
					   !NewState.StateId.Equals(PreviousState.StateId) ||
					   !NewState.NextStart.Equals(PreviousState.NextStart) ||
					   NewState.ScheduledNext != PreviousState.ScheduledNext
					;
			}
		}

		public bool AdherenceFor(string stateCode, ScheduleLayer activity)
		{
			return activity == null || AdherenceFor(stateCode, activity.PayloadId);
		}

		public bool AdherenceFor(string stateCode, Guid activityId)
		{
			var stateGroup = _actualAgentStateAssembler.GetStateGroup(
				stateCode,
				_platformTypeId.HasValue ? _platformTypeId.Value : Guid.Empty,
				_person.BusinessUnitId);
			var alarm = _actualAgentStateAssembler.GetAlarm(activityId, stateGroup.StateGroupId, _person.BusinessUnitId);
			return alarm == null || AdherenceFor(alarm);
		}

		public static bool AdherenceFor(IActualAgentState state)
		{
			return AdherenceFor(state.StaffingEffect);
		}

		public static bool AdherenceFor(RtaAlarmLight alarm)
		{
			return AdherenceFor(alarm.StaffingEffect);
		}

		public static bool AdherenceFor(double staffingEffect)
		{
			return staffingEffect.Equals(0);
		}

		private DateTime startTimeOfShift(ScheduleLayer activity)
		{
			if (activity == null)
				return DateTime.MinValue;
			return activitiesThisShift(activity).Select(x => x.StartDateTime).Min();
		}

		private DateTime endTimeOfShift(ScheduleLayer activity)
		{
			if (activity == null)
				return NewState.ReceivedTime;
			return activitiesThisShift(activity).Select(x => x.EndDateTime).Max();
		}

		private IEnumerable<ScheduleLayer> activitiesThisShift(ScheduleLayer activity)
		{
			return from l in ScheduleLayers
				   where l.BelongsToDate == activity.BelongsToDate
				   select l;
		}

		private ScheduleLayer activityForTime(DateTime time)
		{
			return ScheduleLayers.FirstOrDefault(l => time >= l.StartDateTime && time < l.EndDateTime);
		}

		private ScheduleLayer nextAdjecentActivityToCurrent()
		{
			var nextActivity = (from l in ScheduleLayers where l.StartDateTime > _currentTime select l).FirstOrDefault();
			if (nextActivity == null)
				return null;
			if (CurrentActivity == null)
				return nextActivity;
			if (nextActivity.StartDateTime == CurrentActivity.EndDateTime)
				return nextActivity;
			return null;
		}

	}
}
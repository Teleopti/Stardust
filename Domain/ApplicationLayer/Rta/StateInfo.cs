using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAdherenceAggregatorInfo
	{
		Guid TeamId { get; }
		Guid SiteId { get; }
		IActualAgentState NewState { get; }
		Adherence Adherence { get; }
	}

	public class AdherenceAggregatorInfo : IAdherenceAggregatorInfo
	{
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public IActualAgentState NewState { get; set; }
		public Adherence Adherence { get; set; }
	}

	public enum Adherence
	{
		None,
		In,
		Out
	}

	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly ExternalUserStateInputModel _input;
		private readonly Lazy<IEnumerable<ScheduleLayer>> _scheduleLayers;
		private readonly Lazy<ScheduleLayer> _activityForPreviousState;
		private readonly Lazy<ScheduleLayer> _currentActivity;
		private readonly Lazy<ScheduleLayer> _nextActivityInShift;
		private readonly Lazy<IActualAgentState> _previousState;
		private readonly Lazy<bool> _hasPreviousState;
		private readonly Lazy<DateTime> _currentShiftStartTime;
		private readonly Lazy<DateTime> _currentShiftEndTime;
		private readonly Lazy<DateTime> _shiftStartTimeForPreviousState;
		private readonly Lazy<DateTime> _shiftEndTimeForPreviousState;
		private readonly Lazy<IActualAgentState> _newState;
		private readonly Lazy<Adherence> _adherence;
		private readonly Lazy<Adherence> _adherenceForPreviousState;
		private readonly Lazy<Adherence> _adherenceForPreviousStateAndCurrentActivity;
		private readonly Lazy<Adherence> _adherenceForNewStateAndPreviousActivity;

		private readonly IActualAgentAssembler _actualAgentStateAssembler;
		private readonly PersonOrganizationData _person;
		private readonly DateTime _currentTime;

		public StateInfo(
			IDatabaseReader databaseReader,
			IActualAgentAssembler actualAgentStateAssembler,
			PersonOrganizationData person,
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

			var currentActualAgentState = new Lazy<IActualAgentState>(() => databaseReader.GetCurrentActualAgentState(person.PersonId));
			_previousState = new Lazy<IActualAgentState>(() => currentActualAgentState.Value ??
															   new ActualAgentState
															   {
																   PersonId = person.PersonId,
																   BusinessUnitId = Guid.Empty,
																   StateId = Guid.NewGuid(),
															   });
			_hasPreviousState = new Lazy<bool>(() => currentActualAgentState.Value != null);

			_scheduleLayers = new Lazy<IEnumerable<ScheduleLayer>>(() => databaseReader.GetCurrentSchedule(person.PersonId));
			_activityForPreviousState = new Lazy<ScheduleLayer>(() => activityForTime(PreviousState.ReceivedTime));
			_currentActivity = new Lazy<ScheduleLayer>(() => activityForTime(currentTime));
			_nextActivityInShift = new Lazy<ScheduleLayer>(nextAdjecentActivityToCurrent);
			_currentShiftStartTime = new Lazy<DateTime>(() => startTimeOfShift(CurrentActivity));
			_currentShiftEndTime = new Lazy<DateTime>(() => endTimeOfShift(CurrentActivity));
			_shiftStartTimeForPreviousState = new Lazy<DateTime>(() => startTimeOfShift(ActivityForPreviousState));
			_shiftEndTimeForPreviousState = new Lazy<DateTime>(() => endTimeOfShift(ActivityForPreviousState));

			_adherence = new Lazy<Adherence>(() => AdherenceFor(NewState));
			_adherenceForPreviousState = new Lazy<Adherence>(() =>
			{
				if (_hasPreviousState.Value)
					AdherenceFor(PreviousState);
				return Adherence.None;
			});
			_adherenceForPreviousStateAndCurrentActivity = new Lazy<Adherence>(() => adherenceFor(PreviousState.StateCode, NewState.ScheduledId));
			_adherenceForNewStateAndPreviousActivity = new Lazy<Adherence>(() =>
			{
				var previousActivity = (from l in ScheduleLayers where l.EndDateTime < currentTime select l).LastOrDefault();
				return adherenceFor(_input.StateCode, previousActivity);
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

		public Adherence Adherence { get { return _adherence.Value; } }
		public Adherence AdherenceForPreviousState { get { return _adherenceForPreviousState.Value; } }
		public Adherence AdherenceForPreviousStateAndCurrentActivity { get { return _adherenceForPreviousStateAndCurrentActivity.Value; } }
		public Adherence AdherenceForNewStateAndPreviousActivity { get { return _adherenceForNewStateAndPreviousActivity.Value; } }

		public Guid PersonId { get { return _person.PersonId; } }
		public Guid BusinessUnitId { get { return _person.BusinessUnitId; } }
		public Guid TeamId { get { return _person.TeamId; }}
		public Guid SiteId { get { return _person.SiteId; } }

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

		private Adherence adherenceFor(string stateCode, ScheduleLayer activity)
		{
			if (activity == null)
				return Adherence.None;
			return adherenceFor(stateCode, activity.PayloadId);
		}

		private Adherence adherenceFor(string stateCode, Guid activityId)
		{
			var stateGroup = _actualAgentStateAssembler.GetStateGroup(
				stateCode,
				_input.ParsedPlatformTypeId(),
				_person.BusinessUnitId);
			var alarm = _actualAgentStateAssembler.GetAlarm(activityId, stateGroup.StateGroupId, _person.BusinessUnitId);
			if (alarm == null)
				return Adherence.None;
			return adherenceFor(alarm);
		}

		public static Adherence AdherenceFor(IActualAgentState state)
		{
			return adherenceFor(state.StaffingEffect);
		}

		private static Adherence adherenceFor(RtaAlarmLight alarm)
		{
			return adherenceFor(alarm.StaffingEffect);
		}

		private static Adherence adherenceFor(double staffingEffect)
		{
			return staffingEffect.Equals(0) ? Adherence.In : Adherence.Out;
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
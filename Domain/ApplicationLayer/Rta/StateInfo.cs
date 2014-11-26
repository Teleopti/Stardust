using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class StateInfo
	{
		private readonly ExternalUserStateInputModel _input;
		private readonly Lazy<IEnumerable<ScheduleLayer>> _scheduleLayers;
		private readonly Lazy<ScheduleLayer> _previousActivity;
		private readonly Lazy<ScheduleLayer> _currentActivity;
		private readonly Lazy<ScheduleLayer> _nextActivityInShift;
		private readonly Lazy<IActualAgentState> _previousState;
		private readonly Lazy<DateTime> _currentShiftStartTime;
		private readonly Lazy<DateTime> _currentShiftEndTime;
		private readonly Lazy<DateTime> _previousShiftStartTime;
		private readonly Lazy<DateTime> _previousShiftEndTime;
		private readonly Lazy<bool> _adherenceForNewActivity;
		private readonly Lazy<IActualAgentState> _newState;
		private Lazy<bool> _inAdherence;

		public StateInfo(
			IDatabaseReader databaseReader,
			IActualAgentAssembler actualAgentStateAssembler,
			PersonWithBusinessUnit person,
			ExternalUserStateInputModel input)
		{

			_input = input;
			var time = input.Timestamp;
			var personId = person.PersonId;
			Guid? platformTypeId = null;
			if (input.PlatformTypeId != null)
				platformTypeId = Guid.Parse(input.PlatformTypeId);

			_previousState = new Lazy<IActualAgentState>(() => databaseReader.GetCurrentActualAgentState(personId) ??
															   new ActualAgentState
															   {
																   PersonId = personId,
																   StateId = Guid.NewGuid(),
															   });
			_scheduleLayers = new Lazy<IEnumerable<ScheduleLayer>>(() => databaseReader.GetCurrentSchedule(personId));
			_previousActivity = new Lazy<ScheduleLayer>(() => activityForTime(PreviousState.ReceivedTime));
			_currentActivity = new Lazy<ScheduleLayer>(() => activityForTime(time));
			_nextActivityInShift = new Lazy<ScheduleLayer>(nextAdjecentActivityToCurrent);
			_currentShiftStartTime = new Lazy<DateTime>(() => startTimeOfShift(CurrentActivity));
			_currentShiftEndTime = new Lazy<DateTime>(() => endTimeOfShift(CurrentActivity));
			_previousShiftStartTime = new Lazy<DateTime>(() => startTimeOfShift(PreviousActivity));
			_previousShiftEndTime = new Lazy<DateTime>(() => endTimeOfShift(PreviousActivity));

			_inAdherence = new Lazy<bool>(() => AdherenceFor(NewState));
			_adherenceForNewActivity = new Lazy<bool>(() =>
			{
				if (CurrentActivity != PreviousActivity)
				{
					var stateGroup = actualAgentStateAssembler.GetStateGroup(
						PreviousState.StateCode,
						platformTypeId.HasValue ? platformTypeId.Value : Guid.Empty,
						person.BusinessUnitId);
					var alarm = actualAgentStateAssembler.GetAlarm(NewState.ScheduledId, stateGroup.StateGroupId, person.BusinessUnitId);

					return alarm == null || AdherenceFor(alarm);
				}
				return true;
			});

			var batchId = input.IsSnapshot
				? input.BatchId
				: (DateTime?)null;

			_newState = new Lazy<IActualAgentState>(() => actualAgentStateAssembler.GetAgentState(
				CurrentActivity,
				NextActivityInShift,
				PreviousState,
				personId,
				person.BusinessUnitId,
				platformTypeId,
				input.StateCode,
				input.Timestamp,
				TimeSpan.FromSeconds(input.SecondsInState),
				batchId,
				input.SourceId));

		}

		public IEnumerable<ScheduleLayer> ScheduleLayers { get { return _scheduleLayers.Value; } }
		public IActualAgentState PreviousState { get { return _previousState.Value; } }
		public IActualAgentState NewState { get { return _newState.Value; } }

		public bool IsScheduled { get { return NewState.ScheduledId != Guid.Empty; } }
		public bool WasScheduled { get { return PreviousState.ScheduledId != Guid.Empty; }}

		public DateTime CurrentShiftStartTime { get { return _currentShiftStartTime.Value; } }
		public DateTime CurrentShiftEndTime { get { return _currentShiftEndTime.Value; } }

		public ScheduleLayer CurrentActivity { get { return _currentActivity.Value; } }
		public ScheduleLayer NextActivityInShift { get { return _nextActivityInShift.Value; } }
		public ScheduleLayer PreviousActivity { get { return _previousActivity.Value; } }
		public DateTime PreviousShiftStartTime { get { return _previousShiftStartTime.Value; } }
		public DateTime PreviousShiftEndTime { get { return _previousShiftEndTime.Value; } }

		public bool InAdherence { get { return _inAdherence.Value; } }
		public bool AdherenceForNewActivity { get { return _adherenceForNewActivity.Value; } }

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
			var nextActivity = (from l in ScheduleLayers where l.StartDateTime > _input.Timestamp select l).FirstOrDefault();
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
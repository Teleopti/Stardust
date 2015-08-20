using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public enum AdherenceState
	{
		Unknown,
		In,
		Out,
		Neutral
	}

	public class AdherenceInfo
	{
		private readonly ExternalUserStateInputModel _input;
		private readonly PersonOrganizationData _person;
		private readonly IAppliedAdherence _appliedAdherence;
		private readonly IStateMapper _stateMapper;
		private readonly Lazy<AdherenceState> _adherence;
		private readonly Lazy<AdherenceState> _adherenceForPreviousState;
		private readonly Lazy<AdherenceState> _adherenceForPreviousStateAndCurrentActivity;
		private readonly Lazy<AdherenceState> _adherenceForNewStateAndPreviousActivity;

		public AdherenceInfo(
			ExternalUserStateInputModel input, 
			PersonOrganizationData person,
			Func<PreviousAgentState> previousState,
			Func<CurrentAgentState> currentState,
			ScheduleInfo scheduleInfo, 
			IAppliedAdherence appliedAdherence,
			IStateMapper stateMapper)
		{
			_input = input;
			_person = person;
			_appliedAdherence = appliedAdherence;
			_stateMapper = stateMapper;

			_adherence = new Lazy<AdherenceState>(() => adherenceFor(currentState()));
			_adherenceForPreviousState = new Lazy<AdherenceState>(() => adherenceFor(previousState()));
			_adherenceForPreviousStateAndCurrentActivity = new Lazy<AdherenceState>(() => adherenceFor(previousState().StateCode, previousState().PlatformTypeId, currentState().ActivityId));
			_adherenceForNewStateAndPreviousActivity = new Lazy<AdherenceState>(() => adherenceFor(_input.StateCode, _input.ParsedPlatformTypeId(), scheduleInfo.PreviousActivity()));
		}

		public EventAdherence EventAdherence()
		{
			return _appliedAdherence.StateToEvent(AdherenceState());
		}

		public EventAdherence EventAdherenceForNewStateAndPreviousActivity()
		{
			return _appliedAdherence.StateToEvent(_adherenceForNewStateAndPreviousActivity.Value);
		}

		public EventAdherence AdherenceForPreviousStateAndCurrentActivityForEvent()
		{
			return _appliedAdherence.StateToEvent(_adherenceForPreviousStateAndCurrentActivity.Value);
		}

		public AdherenceState AdherenceState()
		{
			return _adherence.Value;
		}

		public AdherenceState AdherenceForPreviousState()
		{
			return _adherenceForPreviousState.Value;
		}

		public AdherenceState AdherenceForPreviousStateAndCurrentActivity()
		{
			return _adherenceForPreviousStateAndCurrentActivity.Value;
		}
		
		private AdherenceState adherenceFor(CurrentAgentState state)
		{
			return state.Adherence.HasValue ? state.Adherence.Value : Service.AdherenceState.Unknown;
		}

		private AdherenceState adherenceFor(PreviousAgentState state)
		{
			return state.Adherence.HasValue ? state.Adherence.Value : Service.AdherenceState.Unknown;
		}

		private AdherenceState adherenceFor(string stateCode, Guid platformTypeId, ScheduleLayer activity)
		{
			if (activity == null)
				return adherenceFor(stateCode, platformTypeId, (Guid?) null);
			return adherenceFor(stateCode, platformTypeId, activity.PayloadId);
		}

		private AdherenceState adherenceFor(string stateCode, Guid platformTypeId, Guid? activityId)
		{
			var alarm = _stateMapper.AlarmFor(_person.BusinessUnitId, platformTypeId, stateCode, activityId);
			if (alarm == null)
				return Service.AdherenceState.Unknown;
			return alarm.Adherence;
		}

		public static AdherenceState ConvertAdherence(Adherence adherence)
		{
			AdherenceState adherenceState;
			if (!Enum.TryParse(adherence.ToString(), out adherenceState))
				return Service.AdherenceState.Unknown;
			return adherenceState;
		}

		public static AdherenceState AdherenceFor(AgentStateReadModel readModel)
		{
			var staffingEffect = readModel.StaffingEffect;
			if (staffingEffect.HasValue)
				return ConvertAdherence(ByStaffingEffect.ForStaffingEffect(staffingEffect.Value));
			return Service.AdherenceState.Unknown;
		}

	}
}
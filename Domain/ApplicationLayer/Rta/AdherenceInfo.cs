using System;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
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
		private readonly IStateMapper _stateMapper;
		private readonly Lazy<AdherenceState> _adherence;
		private readonly Lazy<AdherenceState> _adherenceForPreviousState;
		private readonly Lazy<AdherenceState> _adherenceForPreviousStateAndCurrentActivity;
		private readonly Lazy<AdherenceState> _adherenceForNewStateAndPreviousActivity;

		public AdherenceInfo(
			ExternalUserStateInputModel input, 
			PersonOrganizationData person, 
			AgentStateInfo agentState, 
			ScheduleInfo scheduleInfo, 
			IStateMapper stateMapper)
		{
			_input = input;
			_person = person;
			_stateMapper = stateMapper;

			_adherence = new Lazy<AdherenceState>(() => adherenceFor(agentState.CurrentState()));
			_adherenceForPreviousState = new Lazy<AdherenceState>(() => adherenceFor(agentState.PreviousState()));
			_adherenceForPreviousStateAndCurrentActivity = new Lazy<AdherenceState>(() => adherenceFor(agentState.PreviousState().StateCode, agentState.CurrentState().ActivityId));
			_adherenceForNewStateAndPreviousActivity = new Lazy<AdherenceState>(() => adherenceFor(_input.StateCode, scheduleInfo.PreviousActivity()));
		}

		public AdherenceState CurrentAdherence()
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

		public AdherenceState AdherenceForNewStateAndPreviousActivity()
		{
			return _adherenceForNewStateAndPreviousActivity.Value;
		}
		
		private AdherenceState adherenceFor(AgentState state)
		{
			return state.Adherence.HasValue ? state.Adherence.Value : AdherenceState.Unknown;
		}

		private AdherenceState adherenceFor(string stateCode, ScheduleLayer activity)
		{
			if (activity == null)
				return adherenceFor(stateCode, (Guid?) null);
			return adherenceFor(stateCode, activity.PayloadId);
		}

		private AdherenceState adherenceFor(string stateCode, Guid? activityId)
		{
			var alarm = _stateMapper.AlarmFor(_person.BusinessUnitId, stateCode, activityId);
			if (alarm == null)
				return AdherenceState.Unknown;
			return alarm.Adherence;
		}

		public static AdherenceState ConvertAdherence(Adherence adherence)
		{
			AdherenceState adherenceState;
			if (!Enum.TryParse(adherence.ToString(), out adherenceState))
				return AdherenceState.Unknown;
			return adherenceState;
		}

		public static AdherenceState AdherenceFor(AgentStateReadModel readModel)
		{
			var staffingEffect = readModel.StaffingEffect;
			if (staffingEffect.HasValue)
				return ConvertAdherence(ByStaffingEffect.ForStaffingEffect(staffingEffect.Value));
			return AdherenceState.Unknown;
		}

	}
}
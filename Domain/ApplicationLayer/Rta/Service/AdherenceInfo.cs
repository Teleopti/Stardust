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
			_adherenceForPreviousStateAndCurrentActivity = new Lazy<AdherenceState>(() => adherenceFor(agentState.PreviousState().StateCode, agentState.PreviousState().PlatformTypeId, agentState.CurrentState().ActivityId));
			_adherenceForNewStateAndPreviousActivity = new Lazy<AdherenceState>(() => adherenceFor(_input.StateCode, _input.ParsedPlatformTypeId(), scheduleInfo.PreviousActivity()));
		}

		public EventAdherence EventAdherence()
		{
			return ConvertToEventAdherence(AdherenceState());
		}

		public EventAdherence EventAdherenceForNewStateAndPreviousActivity()
		{
			return ConvertToEventAdherence(_adherenceForNewStateAndPreviousActivity.Value);
		}

		private EventAdherence ConvertToEventAdherence(AdherenceState adherenceState)
		{
			if (adherenceState == Service.AdherenceState.In)
				return Events.EventAdherence.In;
			if (adherenceState == Service.AdherenceState.Out)
				return Events.EventAdherence.Out;
			return Events.EventAdherence.Neutral;
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
		
		private AdherenceState adherenceFor(AgentState state)
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
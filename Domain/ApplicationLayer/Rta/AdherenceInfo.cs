using System;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public enum Adherence
	{
		None,
		In,
		Out
	}

	public class AdherenceInfo
	{
		private readonly ExternalUserStateInputModel _input;
		private readonly PersonOrganizationData _person;
		private readonly IStateMapper _stateMapper;
		private readonly Lazy<Adherence> _adherence;
		private readonly Lazy<Adherence> _adherenceForPreviousState;
		private readonly Lazy<Adherence> _adherenceForPreviousStateAndCurrentActivity;
		private readonly Lazy<Adherence> _adherenceForNewStateAndPreviousActivity;

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

			_adherence = new Lazy<Adherence>(() => adherenceFor(agentState.CurrentState()));
			_adherenceForPreviousState = new Lazy<Adherence>(() => adherenceFor(agentState.PreviousState()));
			_adherenceForPreviousStateAndCurrentActivity = new Lazy<Adherence>(() => adherenceFor(agentState.PreviousState().StateCode, agentState.CurrentState().ActivityId));
			_adherenceForNewStateAndPreviousActivity = new Lazy<Adherence>(() => adherenceFor(_input.StateCode, scheduleInfo.PreviousActivity()));
		}

		public Adherence CurrentAdherence()
		{
			return _adherence.Value;
		}
		public Adherence AdherenceForPreviousState()
		{
			return _adherenceForPreviousState.Value;
		}

		public Adherence AdherenceForPreviousStateAndCurrentActivity()
		{
			return _adherenceForPreviousStateAndCurrentActivity.Value;
		}

		public Adherence AdherenceForNewStateAndPreviousActivity()
		{
			return _adherenceForNewStateAndPreviousActivity.Value;
		}
		
		public static Adherence AdherenceFor(AgentStateReadModel stateReadModel)
		{
			return adherenceFor(stateReadModel.StaffingEffect);
		}


		private static Adherence adherenceFor(AgentState state)
		{
			return adherenceFor(state.StaffingEffect);
		}

		private static Adherence adherenceFor(AlarmMapping alarmMapping)
		{
			return adherenceFor(alarmMapping.StaffingEffect);
		}

		private static Adherence adherenceFor(double? staffingEffect)
		{
			if (staffingEffect.HasValue)
				return staffingEffect.Value.Equals(0) ? Adherence.In : Adherence.Out;
			return Adherence.None;
		}

		private Adherence adherenceFor(string stateCode, ScheduleLayer activity)
		{
			if (activity == null)
				return Adherence.None;
			return adherenceFor(stateCode, activity.PayloadId);
		}

		private Adherence adherenceFor(string stateCode, Guid? activityId)
		{
			//var stateGroup = _alarmFinder.StateCodeInfoFor(
			//	stateCode, null,
			//	_input.ParsedPlatformTypeId(),
			//	_person.BusinessUnitId);
			//var alarm = _alarmFinder.GetAlarm(activityId, stateGroup.StateGroupId, _person.BusinessUnitId);
			var alarm = _stateMapper.AlarmFor(_person.BusinessUnitId, stateCode, activityId);
			if (alarm == null)
				return Adherence.None;
			return adherenceFor(alarm);
		}

	}
}
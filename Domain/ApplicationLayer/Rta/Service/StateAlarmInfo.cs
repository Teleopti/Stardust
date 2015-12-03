using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateAlarmInfo
	{
		private readonly StateMapping _stateMapping;
		private readonly AlarmMapping _alarmMapping;
		private readonly StoredStateInfo _stored;

		public StateAlarmInfo(
			string stateCode,
			Guid platformTypeId,
			ExternalUserStateInputModel input,
			PersonOrganizationData person,
			StoredStateInfo stored,
			ScheduleInfo schedule,
			IStateMapper stateMapper
			)
		{
			_stateMapping = stateMapper.StateFor(person.BusinessUnitId, platformTypeId, stateCode, input.StateDescription);
			_alarmMapping = stateMapper.AlarmFor(person.BusinessUnitId, platformTypeId, stateCode, schedule.CurrentActivityId()) ?? new AlarmMapping();
			_stored = stored;
		}

		public bool StateGroupChanged()
		{
			return _stateMapping.StateGroupId != _stored.StateGroupId();
		}

		public Guid? StateGroupId()
		{
			return _stateMapping.StateGroupId;
		}

		public string StateGroupName()
		{
			return _stateMapping.StateGroupName;
		}

		public double? StaffingEffect()
		{
			return _alarmMapping.StaffingEffect;
		}

		public Adherence? Adherence()
		{
			return _alarmMapping.Adherence;
		}

		public Guid? RuleId()
		{
			return _alarmMapping.RuleId;
		}

		public string AlarmName()
		{
			return _alarmMapping.AlarmName;
		}

		public long AlarmThresholdTime()
		{
			return _alarmMapping.ThresholdTime;
		}

		public int? AlarmDisplayColor()
		{
			return _alarmMapping.DisplayColor;
		}

	}
}
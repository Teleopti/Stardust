using System;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class StateMapper : IStateMapper
	{
		private readonly ICacheInvalidator _cacheInvalidator;
		private readonly IAlarmMappingLoader _alarmMappingLoader;
		private readonly IStateMappingLoader _stateMappingLoader;
		private readonly IStateCodeAdder _stateCodeAdder;

		public StateMapper(
			ICacheInvalidator cacheInvalidator,
			IAlarmMappingLoader alarmMappingLoader,
			IStateMappingLoader stateMappingLoader,
			IStateCodeAdder stateCodeAdder)
		{
			_cacheInvalidator = cacheInvalidator;
			_alarmMappingLoader = alarmMappingLoader;
			_stateMappingLoader = stateMappingLoader;
			_stateCodeAdder = stateCodeAdder;
		}

		public AlarmMapping AlarmFor(Guid businessUnitId, string stateCode, Guid? activityId)
		{
			var match = queryAlarm(businessUnitId, stateCode, activityId);
			if (activityId != null && match == null)
				match = queryAlarm(businessUnitId, stateCode, null);
			return match;
		}

		private AlarmMapping queryAlarm(Guid businessUnitId, string stateCode, Guid? activityId)
		{
			return (from m in _alarmMappingLoader.Load()
				where
					m.BusinessUnitId == businessUnitId &&
					m.StateCode == stateCode &&
					m.ActivityId == activityId
				select m)
				.SingleOrDefault();
		}

		public StateMapping StateFor(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			var match = queryState(businessUnitId, platformTypeId, stateCode);
			if (match != null) return match;
			_stateCodeAdder.AddUnknownStateCode(businessUnitId, platformTypeId, stateCode, stateDescription);
			_cacheInvalidator.Invalidate();
			match = queryState(businessUnitId, platformTypeId, stateCode);
			if (match != null) return match;
			return new StateMapping
			{
				BusinessUnitId = businessUnitId,
				PlatformTypeId = platformTypeId,
				StateCode = stateCode
			};
		}

		private StateMapping queryState(Guid businessUnitId, Guid platformTypeId, string stateCode)
		{
			return (from m in _stateMappingLoader.Load()
				where
					m.BusinessUnitId == businessUnitId &&
					m.PlatformTypeId == platformTypeId &&
					m.StateCode == stateCode
				select m)
				.SingleOrDefault();
		}

	}
}
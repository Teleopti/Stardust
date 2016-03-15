using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class MappedState
	{
		public Guid StateGroupId { get; set; }
		public string StateGroupName { get; set; }
		public bool IsLogOutState { get; set; }
	}

	public class MappedRule
	{
		public Guid RuleId { get; set; }
		public string RuleName { get; set; }
		public Adherence? Adherence { get; set; }
		public int? StaffingEffect { get; set; }
		public int DisplayColor { get; set; }

		public bool IsAlarm { get; set; }
		public long ThresholdTime { get; set; }
		public int AlarmColor { get; set; }
	}

	public class StateMapper
	{
		private readonly ICacheInvalidator _cacheInvalidator;
		private readonly IStateCodeAdder _stateCodeAdder;

		public StateMapper(
			ICacheInvalidator cacheInvalidator,
			IStateCodeAdder stateCodeAdder)
		{
			_cacheInvalidator = cacheInvalidator;
			_stateCodeAdder = stateCodeAdder;
		}

		public MappedRule RuleFor(IEnumerable<Mapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, Guid? activityId)
		{
			var match = queryRule(mappings, businessUnitId, platformTypeId, stateCode, activityId);
			if (activityId != null && match == null)
				match = queryRule(mappings, businessUnitId, platformTypeId, stateCode, null);
			return match;
		}

		private MappedRule queryRule(IEnumerable<Mapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, Guid? activityId)
		{
			return (from m in mappings
				where
					m.BusinessUnitId == businessUnitId &&
					m.PlatformTypeId == platformTypeId &&
					m.StateCode == stateCode &&
					m.ActivityId == activityId
				select new MappedRule
				{
					RuleId = m.RuleId,
					RuleName = m.RuleName,
					Adherence = m.Adherence,
					StaffingEffect = m.StaffingEffect,
					DisplayColor = m.DisplayColor,
					IsAlarm = m.IsAlarm,
					ThresholdTime = m.ThresholdTime,
					AlarmColor = m.AlarmColor
				})
				.SingleOrDefault();
		}

		//public Guid RuleId { get; set; }
		//public string RuleName { get; set; }
		//public Adherence? Adherence { get; set; }
		//public int? StaffingEffect { get; set; }
		//public int DisplayColor { get; set; }

		//public bool IsAlarm { get; set; }
		//public long ThresholdTime { get; set; }
		//public int AlarmColor { get; set; }

		public MappedState StateFor(IEnumerable<Mapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			if (stateCode == null)
				return noMatchState(businessUnitId, platformTypeId, null);
			var match = queryState(mappings, businessUnitId, platformTypeId, stateCode);
			if (match != null) return match;
			_stateCodeAdder.AddUnknownStateCode(businessUnitId, platformTypeId, stateCode, stateDescription);
			_cacheInvalidator.InvalidateState();
			match = queryState(mappings, businessUnitId, platformTypeId, stateCode);
			return match ?? noMatchState(businessUnitId, platformTypeId, stateCode);
		}

		private static MappedState noMatchState(Guid businessUnitId, Guid platformTypeId, string stateCode)
		{
			return new MappedState
			{
				//BusinessUnitId = businessUnitId,
				//PlatformTypeId = platformTypeId,
				//StateCode = stateCode
			};
		}

		private MappedState queryState(IEnumerable<Mapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode)
		{
			return (from m in mappings
				where
					m.BusinessUnitId == businessUnitId &&
					m.PlatformTypeId == platformTypeId &&
					m.StateCode == stateCode
				select new MappedState
				{
					StateGroupId = m.StateGroupId,
					StateGroupName = m.StateGroupName,
					IsLogOutState = m.IsLogOutState
				})
				.FirstOrDefault();
		}

	}
}
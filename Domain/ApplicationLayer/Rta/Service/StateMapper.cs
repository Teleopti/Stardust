using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
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

		public RuleMapping RuleFor(IEnumerable<RuleMapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, Guid? activityId)
		{
			var match = queryRule(mappings, businessUnitId, platformTypeId, stateCode, activityId);
			if (activityId != null && match == null)
				match = queryRule(mappings, businessUnitId, platformTypeId, stateCode, null);
			return match;
		}

		private RuleMapping queryRule(IEnumerable<RuleMapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, Guid? activityId)
		{
			return (from m in mappings
					where
					m.BusinessUnitId == businessUnitId &&
					m.PlatformTypeId == platformTypeId &&
					m.StateCode == stateCode &&
					m.ActivityId == activityId
				select m)
				.SingleOrDefault();
		}

		public StateMapping StateFor(IEnumerable<StateMapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
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

		private static StateMapping noMatchState(Guid businessUnitId, Guid platformTypeId, string stateCode)
		{
			return new StateMapping
			{
				BusinessUnitId = businessUnitId,
				PlatformTypeId = platformTypeId,
				StateCode = stateCode
			};
		}

		private StateMapping queryState(IEnumerable<StateMapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode)
		{
			return (from m in mappings
					where
					m.BusinessUnitId == businessUnitId &&
					m.PlatformTypeId == platformTypeId &&
					m.StateCode == stateCode
				select m)
				.SingleOrDefault();
		}

	}
}
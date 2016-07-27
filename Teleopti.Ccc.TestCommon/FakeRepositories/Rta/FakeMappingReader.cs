using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeMappingReader : IMappingReader
	{
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IActivityRepository _activities;
		private readonly IRtaMapRepository _mapRepository;
		private readonly IRtaStateGroupRepository _stateGroups;
		private readonly IToggleManager _toggles;

		private IEnumerable<Mapping> _mappings;

		public FakeMappingReader(
			IBusinessUnitRepository businessUnits,
			IActivityRepository activities,
			IRtaMapRepository mapRepository,
			IRtaStateGroupRepository stateGroups,
			IToggleManager toggles
			)
		{
			_businessUnits = businessUnits;
			_activities = activities;
			_mapRepository = mapRepository;
			_stateGroups = stateGroups;
			_toggles = toggles;
		}

		public IEnumerable<Mapping> Read()
		{
			var mappingsExpression = MappingReadModelUpdater.MakeMappings(_businessUnits, _activities, _stateGroups, _mapRepository);

			// mimic the new mapping read models eventual updating
			if (_toggles.IsEnabled(Toggles.RTA_RuleMappingOptimization_39812))
				return _mappings ?? (_mappings = mappingsExpression.ToArray());

			// mimic the view kinda (will include all combinations, it really shouldnt)
			return mappingsExpression.ToArray();
		}

		public IEnumerable<Mapping> ReadFor(IEnumerable<string> stateCodes, IEnumerable<Guid?> activities)
		{
			return (
				from m in Read()
				where
					stateCodes.Contains(m.StateCode) &&
					activities.Contains(m.ActivityId)
				select m
				);
		}
	}
}

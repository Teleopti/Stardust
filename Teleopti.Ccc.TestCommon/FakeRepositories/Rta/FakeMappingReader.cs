using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeMappingReader : IMappingReader
	{
		private readonly IBusinessUnitRepository _businessUnits;
		private readonly IActivityRepository _activities;
		private readonly IRtaMapRepository _mapRepository;
		private readonly IRtaStateGroupRepository _stateGroups;

		public FakeMappingReader(
			IBusinessUnitRepository businessUnits,
			IActivityRepository activities,
			IRtaMapRepository mapRepository,
			IRtaStateGroupRepository stateGroups
			)
		{
			_businessUnits = businessUnits;
			_activities = activities;
			_mapRepository = mapRepository;
			_stateGroups = stateGroups;
		}

		public IEnumerable<Mapping> Read()
		{
			return MappingReadModelUpdater.MakeMappings(_businessUnits, _activities, _stateGroups, _mapRepository);
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

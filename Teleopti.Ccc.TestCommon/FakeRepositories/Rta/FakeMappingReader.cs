using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeMappingReader : IMappingReader
	{
		private readonly IRtaMapRepository _rtaMapRepository;
		private readonly IAppliedAdherence _appliedAdherence;
		private readonly IRtaStateGroupRepository _stateGroupRepository;

		public FakeMappingReader(
			IRtaMapRepository rtaMapRepository,
			IAppliedAdherence appliedAdherence,
			IRtaStateGroupRepository stateGroupRepository)
		{
			_rtaMapRepository = rtaMapRepository;
			_appliedAdherence = appliedAdherence;
			_stateGroupRepository = stateGroupRepository;
		}

		public IEnumerable<Mapping> Read()
		{
			// reuse the complicated linq statement
			// dodge the aspect
			// when mapping loader is removed, move the linq statement here
			return new MappingLoader(_rtaMapRepository, _appliedAdherence, _stateGroupRepository)
				.Load();
		}

		public IEnumerable<Mapping> ReadFor(IEnumerable<string> stateCodes, IEnumerable<Guid?> activities)
		{
			// mimics dbo.LoadRtaMappingsFor.sql .. maybe
			// if this is complex, just return all, but to mimics gives a better test suite
			var mappings = Read();
			var combinations = (
				from m in mappings
				where
					(m.StateCode == null || stateCodes.Contains(m.StateCode)) &&
					(m.ActivityId == null || activities.Contains(m.ActivityId))
				select m
				).ToArray();
			var unmappedStates = (
				from c in stateCodes
				let m = mappings.FirstOrDefault(x => x.StateCode == c)
				where m != null
				select m
				).ToArray();
			return combinations
				.Union(unmappedStates);
		}
	}
}

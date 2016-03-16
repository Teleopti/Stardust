using System.Collections.Generic;
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
	}
}
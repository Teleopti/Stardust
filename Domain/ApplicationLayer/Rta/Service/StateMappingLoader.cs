using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateMappingLoader : IStateMappingLoader
	{
		private readonly IRtaStateGroupRepository _stateGroupRepository;
		private Guid x = Guid.NewGuid();

		public StateMappingLoader(IRtaStateGroupRepository stateGroupRepository)
		{
			_stateGroupRepository = stateGroupRepository;
		}

		public virtual IEnumerable<StateMapping> Cached()
		{
			return Load();
		}

		[AllBusinessUnitsUnitOfWork]
		public virtual IEnumerable<StateMapping> Load()
		{
			var groups = _stateGroupRepository.LoadAllCompleteGraph();
			return (
				from g in groups
				from s in g.StateCollection
				select new StateMapping
				{
					BusinessUnitId = g.BusinessUnit.Id.Value,
					PlatformTypeId = s.PlatformTypeId,
					StateCode = s.StateCode,

					StateGroupId = g.Id.Value,
					StateGroupName = g.Name,
					IsLogOutState = g.IsLogOutState
				}
				).ToArray();
		}
	}
}
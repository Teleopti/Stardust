using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateCodeAdder
	{
		MappedState AddUnknownStateCode(IEnumerable<Mapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription);
	}
	
	public class EventualStateCodeAdder : IStateCodeAdder
	{
		private readonly IRtaStateGroupRepository _stateGroupRepository;
		private readonly object defaultStateGroupLock = new object();

		public EventualStateCodeAdder(IRtaStateGroupRepository stateGroupRepository)
		{
			_stateGroupRepository = stateGroupRepository;
		}
		
		public MappedState AddUnknownStateCode(IEnumerable<Mapping> mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			var stateGroups = _stateGroupRepository.LoadAllCompleteGraph();

			IRtaStateGroup existingStateGroup;
			lock (defaultStateGroupLock)
			{
				existingStateGroup = (
					from g in stateGroups
					from s in g.StateCollection
					where g.BusinessUnit.Id.Value == businessUnitId
						  && s.StateCode == stateCode
					select g
					).SingleOrDefault();
			}

			if (existingStateGroup != null)
				return new MappedState
				{
					StateGroupId = existingStateGroup.Id.Value,
					StateGroupName = existingStateGroup.Name
				};

			var defaultStateGroup = (
				from g in stateGroups
				where g.BusinessUnit.Id.Value == businessUnitId &&
					  g.DefaultStateGroup
				select g
				).SingleOrDefault();

			if (defaultStateGroup != null)
			{
				lock (defaultStateGroupLock)
					defaultStateGroup.AddState(stateDescription ?? stateCode, stateCode, platformTypeId);
				return new MappedState
				{
					StateGroupId = defaultStateGroup.Id.Value,
					StateGroupName = defaultStateGroup.Name
				};
			}

			return null;
		}
	}
}
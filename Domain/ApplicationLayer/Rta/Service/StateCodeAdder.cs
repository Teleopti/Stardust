using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateCodeAdder : IStateCodeAdder
	{		
		private readonly IRtaStateGroupRepository _stateGroupRepository;

		public StateCodeAdder(IRtaStateGroupRepository stateGroupRepository)
		{
			_stateGroupRepository = stateGroupRepository;
		}

		[AllBusinessUnitsUnitOfWork]
		public virtual void AddUnknownStateCode(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			var defaultStateGroup = (from g in _stateGroupRepository.LoadAll()
				where g.BusinessUnit.Id.Value == businessUnitId &&
				      g.DefaultStateGroup
				select g).SingleOrDefault();
			if (defaultStateGroup != null)
				defaultStateGroup.AddState(stateDescription, stateCode, platformTypeId);
		}
	}
}
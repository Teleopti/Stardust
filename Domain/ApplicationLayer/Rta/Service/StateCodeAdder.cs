using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateCodeAdder : IStateCodeAdder
	{		
		private readonly IRtaStateGroupRepository _stateGroupRepository;
		private readonly ICurrentUnitOfWork _unitOfWork;

		public StateCodeAdder(IRtaStateGroupRepository stateGroupRepository, ICurrentUnitOfWork unitOfWork)
		{
			_stateGroupRepository = stateGroupRepository;
			_unitOfWork = unitOfWork;
		}

		public virtual void AddUnknownStateCode(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			var defaultStateGroup = (from g in _stateGroupRepository.LoadAll()
				where g.BusinessUnit.Id.Value == businessUnitId &&
				      g.DefaultStateGroup
				select g).SingleOrDefault();
			if (defaultStateGroup != null)
				defaultStateGroup.AddState(stateDescription ?? stateCode, stateCode, platformTypeId);

			// have to persist to reload
			_unitOfWork.Current().PersistAll();
		}
	}
}
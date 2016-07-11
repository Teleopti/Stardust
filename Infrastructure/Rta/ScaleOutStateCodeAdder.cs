using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class ScaleOutStateCodeAdder : IStateCodeAdder
	{
		private readonly IRtaStateGroupRepository _stateGroupRepository;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IDistributedLockAcquirer _locker;

		public ScaleOutStateCodeAdder(
			IRtaStateGroupRepository stateGroupRepository, 
			ICurrentUnitOfWork unitOfWork,
			IDistributedLockAcquirer locker)
		{
			_stateGroupRepository = stateGroupRepository;
			_unitOfWork = unitOfWork;
			_locker = locker;
		}

		public virtual void AddUnknownStateCode(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			using (_locker.LockForTypeOf(this))
			{
				var defaultStateGroup = (from g in _stateGroupRepository.LoadAll()
										 where g.BusinessUnit.Id.Value == businessUnitId &&
											   g.DefaultStateGroup
										 select g).SingleOrDefault();
				if (defaultStateGroup != null && defaultStateGroup.StateCollection.All(x => x.StateCode != stateCode))
					defaultStateGroup.AddState(stateDescription ?? stateCode, stateCode, platformTypeId);

				// have to persist to reload
				_unitOfWork.Current().PersistAll();
			}			
		}
	}
}
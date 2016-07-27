using System;
using System.Linq;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateCodeAdder
	{
		MappedState AddUnknownStateCode(MappingsState mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription);
	}

	public class StateCodeAdder : IStateCodeAdder
	{		
		private readonly IRtaStateGroupRepository _stateGroupRepository;
		private readonly ICurrentUnitOfWork _unitOfWork;

		public StateCodeAdder(IRtaStateGroupRepository stateGroupRepository, ICurrentUnitOfWork unitOfWork)
		{
			_stateGroupRepository = stateGroupRepository;
			_unitOfWork = unitOfWork;
		}

		public MappedState AddUnknownStateCode(MappingsState mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			MappedState mapped = null;

			var defaultStateGroup = (
				from g in _stateGroupRepository.LoadAll()
				where g.BusinessUnit.Id.Value == businessUnitId &&
					  g.DefaultStateGroup
				select g
				).SingleOrDefault();

			if (defaultStateGroup != null)
			{
				defaultStateGroup.AddState(stateDescription ?? stateCode, stateCode, platformTypeId);
				mapped = new MappedState
				{
					StateGroupId = defaultStateGroup.Id.Value,
					StateGroupName = defaultStateGroup.Name
				};
			}

			// persist and invalidate
			_unitOfWork.Current().PersistAll();
			mappings.Invalidate();

			return mapped;
		}
	}



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

		public MappedState AddUnknownStateCode(MappingsState mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			using (_locker.LockForTypeOf(this))
			{
				MappedState mapped = null;

				var defaultStateGroup = (
					from g in _stateGroupRepository.LoadAll()
					where g.BusinessUnit.Id.Value == businessUnitId &&
						  g.DefaultStateGroup
					select g
					).SingleOrDefault();

				if (defaultStateGroup != null)
				{
					var exists = defaultStateGroup.StateCollection.Any(x => x.StateCode == stateCode);
					if (!exists)
						defaultStateGroup.AddState(stateDescription ?? stateCode, stateCode, platformTypeId);

					mapped = new MappedState
					{
						StateGroupId = defaultStateGroup.Id.Value,
						StateGroupName = defaultStateGroup.Name
					};
				}

				// have to persist to reload
				_unitOfWork.Current().PersistAll();
				mappings.Invalidate();

				return mapped;
			}
		}
	}




	public class EventualStateCodeAdder : IStateCodeAdder
	{
		private readonly IRtaStateGroupRepository _stateGroupRepository;

		public EventualStateCodeAdder(IRtaStateGroupRepository stateGroupRepository)
		{
			_stateGroupRepository = stateGroupRepository;
		}

		public MappedState AddUnknownStateCode(MappingsState mappings, Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription)
		{
			lock (this)
			{
				var existingStateGroup = (
					from g in _stateGroupRepository.LoadAll()
					from s in g.StateCollection
					where g.BusinessUnit.Id.Value == businessUnitId
						  && s.StateCode == stateCode
					select g
					).SingleOrDefault();

				if (existingStateGroup != null)
					return new MappedState
					{
						StateGroupId = existingStateGroup.Id.Value,
						StateGroupName = existingStateGroup.Name
					};

				var defaultStateGroup = (
					from g in _stateGroupRepository.LoadAll()
					where g.BusinessUnit.Id.Value == businessUnitId &&
						  g.DefaultStateGroup
					select g
					).SingleOrDefault();

				if (defaultStateGroup != null)
				{
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
}
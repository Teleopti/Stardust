using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Persisters.WriteProtection
{
	public class WriteProtectionPersister : IWriteProtectionPersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IWriteProtectionRepository _writeProtectionRepository;
		private readonly IInitiatorIdentifier _initiatorIdentifier;

		public WriteProtectionPersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
																	IWriteProtectionRepository writeProtectionRepository,
																	IInitiatorIdentifier initiatorIdentifier)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_writeProtectionRepository = writeProtectionRepository;
			_initiatorIdentifier = initiatorIdentifier;
		}

		public void Persist(ICollection<IPersonWriteProtectionInfo> writeProtections)
		{
			using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_writeProtectionRepository.AddRange(writeProtections);
				unitOfWork.PersistAll(_initiatorIdentifier);
				writeProtections.Clear();
			}
		}
	}
}
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.WriteProtection
{
	public class WriteProtectionPersister : IWriteProtectionPersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IWriteProtectionRepository _writeProtectionRepository;
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;

		public WriteProtectionPersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
																	IWriteProtectionRepository writeProtectionRepository,
																	IMessageBrokerIdentifier messageBrokerIdentifier)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_writeProtectionRepository = writeProtectionRepository;
			_messageBrokerIdentifier = messageBrokerIdentifier;
		}

		public void Persist(ICollection<IPersonWriteProtectionInfo> writeProtections)
		{
			using (var unitOfWork = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				_writeProtectionRepository.AddRange(writeProtections);
				unitOfWork.PersistAll(_messageBrokerIdentifier);
				writeProtections.Clear();
			}
		}
	}
}
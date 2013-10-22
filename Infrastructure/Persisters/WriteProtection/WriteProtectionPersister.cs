using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.WriteProtection
{
	public class WriteProtectionPersister : IWriteProtectionPersister
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IWriteProtectionRepository _writeProtectionRepository;
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;

		public WriteProtectionPersister(IUnitOfWorkFactory unitOfWorkFactory, 
																	IWriteProtectionRepository writeProtectionRepository,
																	IMessageBrokerIdentifier messageBrokerIdentifier)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_writeProtectionRepository = writeProtectionRepository;
			_messageBrokerIdentifier = messageBrokerIdentifier;
		}

		public void Persist(ICollection<IPersonWriteProtectionInfo> writeProtections)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				_writeProtectionRepository.AddRange(writeProtections);
				unitOfWork.PersistAll(_messageBrokerIdentifier);
				writeProtections.Clear();
			}
		}
	}
}
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public class PersonAccountPersister : IPersonAccountPersister
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;

		public PersonAccountPersister(IUnitOfWorkFactory unitOfWorkFactory, 
																IPersonAbsenceAccountRepository personAbsenceAccountRepository,
																IMessageBrokerIdentifier messageBrokerIdentifier)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_messageBrokerIdentifier = messageBrokerIdentifier;
		}

		public void Persist(ICollection<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork(TransactionIsolationLevel.Serializable))
			{
				_personAbsenceAccountRepository.AddRange(personAbsenceAccounts);
				uow.PersistAll(_messageBrokerIdentifier);				
			}
			personAbsenceAccounts.Clear();
		}
	}
}
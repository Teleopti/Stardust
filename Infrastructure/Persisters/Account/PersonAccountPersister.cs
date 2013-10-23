using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public class PersonAccountPersister : IPersonAccountPersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IMessageBrokerIdentifier _messageBrokerIdentifier;
		private readonly IPersonAccountConflictCollector _personAccountConflictCollector;
		private readonly IPersonAccountConflictResolver _personAccountConflictResolver;

		public PersonAccountPersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
																IPersonAbsenceAccountRepository personAbsenceAccountRepository,
																IMessageBrokerIdentifier messageBrokerIdentifier,
																IPersonAccountConflictCollector personAccountConflictCollector,
																IPersonAccountConflictResolver personAccountConflictResolver)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_messageBrokerIdentifier = messageBrokerIdentifier;
			_personAccountConflictCollector = personAccountConflictCollector;
			_personAccountConflictResolver = personAccountConflictResolver;
		}

		public void Persist(ICollection<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork(TransactionIsolationLevel.Serializable))
			{
				var conflictingPersonAccounts = _personAccountConflictCollector.GetConflicts(personAbsenceAccounts);
				_personAccountConflictResolver.Resolve(conflictingPersonAccounts);
				_personAbsenceAccountRepository.AddRange(personAbsenceAccounts);
				uow.PersistAll(_messageBrokerIdentifier);				
			}
			personAbsenceAccounts.Clear();
		}
	}
}
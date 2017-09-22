using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Persisters.Account
{
	public class PersonAccountPersister : IPersonAccountPersister
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private readonly IInitiatorIdentifier _initiatorIdentifier;
		private readonly IPersonAccountConflictCollector _personAccountConflictCollector;
		private readonly IPersonAccountConflictResolver _personAccountConflictResolver;

		public PersonAccountPersister(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, 
																IPersonAbsenceAccountRepository personAbsenceAccountRepository,
																IInitiatorIdentifier initiatorIdentifier,
																IPersonAccountConflictCollector personAccountConflictCollector,
																IPersonAccountConflictResolver personAccountConflictResolver)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
			_initiatorIdentifier = initiatorIdentifier;
			_personAccountConflictCollector = personAccountConflictCollector;
			_personAccountConflictResolver = personAccountConflictResolver;
		}

		public bool Persist(ICollection<IPersonAbsenceAccount> personAbsenceAccounts)
		{
			bool hadConflicts;
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var conflictingPersonAccounts = _personAccountConflictCollector.GetConflicts(personAbsenceAccounts);
				hadConflicts = conflictingPersonAccounts.Any();
				_personAccountConflictResolver.Resolve(conflictingPersonAccounts);
				_personAbsenceAccountRepository.AddRange(personAbsenceAccounts);
				uow.PersistAll(_initiatorIdentifier);
			}
			personAbsenceAccounts.Clear();
			return hadConflicts;
		}
	}
}
using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class PersonAccountUpdater : IPersonAccountUpdater
    {
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
		
		public PersonAccountUpdater(IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
		}


		public void UpdateOnTermination(DateOnly terminalDate, IPerson person)
		{

			// check for authorization???

			// load accounts
			var accounts = _personAbsenceAccountRepository.Find(person);
			foreach (var account in accounts)
			{
				foreach (var temp in account.AccountCollection())
				{
					//temp.LatestCalculatedBalance()
				}
			}

				//checkIfAuthorized(foundPerson, dateFrom);

				//var accounts = _personAbsenceAccountRepository.Find(foundPerson);
				//var personAccount = accounts.Find(foundAbsence, dateFrom);
				//if (personAccount == null || !personAccount.StartDate.Equals(dateFrom))
				//{
				//	personAccount = createPersonAccount(foundAbsence, accounts, dateFrom);
				//}

				//setPersonAccount(personAccount, command);
                            
				//unitOfWork.PersistAll();


		}

		public void UpdateOnActivation(IPerson person)
		{
			throw new NotImplementedException();
		}
	}
}
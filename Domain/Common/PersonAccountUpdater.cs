using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class PersonAccountUpdater : IPersonAccountUpdater
	{
		private readonly IEnumerable<IAccount> _personAbsenceAccounts;

		public PersonAccountUpdater(IEnumerable<IAccount> personAbsenceAccounts)
		{
			_personAbsenceAccounts = personAbsenceAccounts;
		}

		public void UpdatePersonAccounts(DateTime? terminalDate)
		{
			// sort person account collection by starting datetime
			// go though all of them
			// set the account 
			throw new System.NotImplementedException();
		}
	}
}
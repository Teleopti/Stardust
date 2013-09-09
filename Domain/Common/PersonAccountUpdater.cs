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
			throw new NotImplementedException();
		}

		public void UpdateOnActivation(IPerson person)
		{
			throw new NotImplementedException();
		}
	}
}
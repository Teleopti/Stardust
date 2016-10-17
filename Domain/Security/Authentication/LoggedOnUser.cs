﻿using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Authentication
{
	public class LoggedOnUser : ILoggedOnUser
	{
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public LoggedOnUser(IPersonRepository personRepository, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_personRepository = personRepository;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		public IPerson CurrentUser()
		{
			var pricipal = _currentTeleoptiPrincipal.Current();
			return pricipal?.GetPerson(_personRepository);
		}

	}
}
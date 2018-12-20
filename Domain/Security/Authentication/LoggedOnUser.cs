using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

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
			var principal = _currentTeleoptiPrincipal.Current();
			if (principal == null)
				return null;
			return _personRepository?.Get(principal.PersonId);
		}

	}
}
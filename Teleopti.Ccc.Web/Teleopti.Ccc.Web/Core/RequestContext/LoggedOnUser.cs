using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
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
			return _currentTeleoptiPrincipal.Current().GetPerson(_personRepository);
		}

		public ITeam MyTeam(DateOnly date)
		{
			return CurrentUser().MyTeam(date);
		}
	}
}
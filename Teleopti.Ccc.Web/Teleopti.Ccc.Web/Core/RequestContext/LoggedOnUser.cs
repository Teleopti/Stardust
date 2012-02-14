using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class LoggedOnUser : ILoggedOnUser
	{
		private readonly IPersonRepository _personRepository;

		public LoggedOnUser(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		public IPerson CurrentUser()
		{
			return TeleoptiPrincipal.Current.GetPerson(_personRepository);
		}

		public ITeam MyTeam(DateOnly date)
		{
			return CurrentUser().MyTeam(date);
		}
	}
}
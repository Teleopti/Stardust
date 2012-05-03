using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class LoggedOnUser : ILoggedOnUser
	{
		private readonly IPersonRepository _personRepository;
		private readonly IPrincipalProvider _principalProvider;

		public LoggedOnUser(IPersonRepository personRepository, IPrincipalProvider principalProvider)
		{
			_personRepository = personRepository;
			_principalProvider = principalProvider;
		}

		public IPerson CurrentUser()
		{
			return _principalProvider.Current().GetPerson(_personRepository);
		}

		public ITeam MyTeam(DateOnly date)
		{
			return CurrentUser().MyTeam(date);
		}
	}
}
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class RegionalFactory : IRegionalFactory
	{
		public IRegional MakeRegional(IPerson loggedOnUser)
		{
			return Regional.FromPerson(loggedOnUser);
		}
	}
}
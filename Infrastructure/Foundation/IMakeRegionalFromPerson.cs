using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IMakeRegionalFromPerson
	{
		IRegional MakeRegionalFromPerson(IPerson loggedOnUser);
	}
}
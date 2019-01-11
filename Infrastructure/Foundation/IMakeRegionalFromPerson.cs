using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IMakeRegionalFromPerson
	{
		IRegional MakeRegionalFromPerson(IPerson person);
	}
}
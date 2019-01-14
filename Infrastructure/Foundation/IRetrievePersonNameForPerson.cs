using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IRetrievePersonNameForPerson
	{
		string NameForPerson(IPrincipalSource person);
	}
}
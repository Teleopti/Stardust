using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IRetrievePersonNameForPerson
	{
		string NameForPerson(IPerson person);
	}
}
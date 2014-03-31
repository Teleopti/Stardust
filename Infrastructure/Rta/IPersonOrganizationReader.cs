using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public interface IPersonOrganizationReader
	{
		IEnumerable<PersonOrganizationData> LoadAll();
	}
}
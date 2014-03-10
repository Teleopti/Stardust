using System.Collections.Generic;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public interface IPersonOrganizationReader
	{
		IEnumerable<PersonOrganizationData> LoadAll();
	}
}
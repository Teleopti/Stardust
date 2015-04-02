using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IPersonOrganizationReader
	{
		IEnumerable<PersonOrganizationData> PersonOrganizationData();
	}
}
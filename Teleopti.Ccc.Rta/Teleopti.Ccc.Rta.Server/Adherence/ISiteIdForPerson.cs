using System;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public interface ISiteIdForPerson
	{
		Guid GetSiteId(Guid personId);
	}
}
using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IFindExternalApplicationAccess
	{
		ExternalApplicationAccess FindByTokenHash(string hash);
		IEnumerable<ExternalApplicationAccess> FindByPerson(Guid personId);
	}
}
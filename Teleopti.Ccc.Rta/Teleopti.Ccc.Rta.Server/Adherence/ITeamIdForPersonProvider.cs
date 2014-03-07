using System;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public interface ITeamIdForPersonProvider
	{
		Guid GetTeamId(Guid personId);
	}
}
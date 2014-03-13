using System;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public interface ITeamIdForPerson
	{
		Guid GetTeamId(Guid personId);
	}
}
using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IIdUserQuery
	{
		PersonInfo FindUserData(Guid id);
	}
}
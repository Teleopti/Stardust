using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IFindPersonInfoByCredentials
	{
		PersonInfo Find(Guid personId, string tenantPassword);
	}
}
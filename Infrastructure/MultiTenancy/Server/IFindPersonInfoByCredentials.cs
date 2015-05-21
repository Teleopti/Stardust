using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IFindPersonInfoByCredentials
	{
		PersonInfo Find(Guid personId, string tenantPassword);
	}
}
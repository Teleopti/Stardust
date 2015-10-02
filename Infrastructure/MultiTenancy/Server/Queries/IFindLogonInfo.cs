using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IFindLogonInfo
	{
		IEnumerable<LogonInfo> GetForIds(IEnumerable<Guid> ids);
		LogonInfo GetForLogonName(string logonName);
	}
}
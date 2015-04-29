using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IFindLogonInfo
	{
		IEnumerable<LogonInfo> GetForIds(IEnumerable<Guid> ids);
	}
}
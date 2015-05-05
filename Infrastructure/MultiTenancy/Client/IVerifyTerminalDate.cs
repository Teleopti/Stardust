using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IVerifyTerminalDate
	{
		bool IsTerminated(string tenantName, Guid personId);
	}
}
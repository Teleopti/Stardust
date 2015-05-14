using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class VerifyTerminalDateFake : IVerifyTerminalDate
	{
		public bool IsTerminated(string tenantName, Guid personId)
		{
			return false;
		}
	}
}
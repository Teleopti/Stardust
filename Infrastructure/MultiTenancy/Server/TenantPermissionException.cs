using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class TenantPermissionException : Exception
	{
		public TenantPermissionException(string message) : base(message)
		{
		}
	}
}
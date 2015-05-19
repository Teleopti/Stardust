using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.WinCode.Main
{
	public class WinTenantCredentials : ICurrentTenantCredentials
	{
		private static TenantCredentials _tenantCredentials;

		public TenantCredentials TenantCredentials()
		{
			return _tenantCredentials;
		}

		public static void SetCredentials(Guid personId, string tenantPassword)
		{
			_tenantCredentials = new TenantCredentials(personId, tenantPassword);
		}

		public static void Clear()
		{
			_tenantCredentials = null;
		}
	}
}
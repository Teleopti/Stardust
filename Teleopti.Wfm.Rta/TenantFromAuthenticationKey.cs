using System;
using Microsoft.CSharp.RuntimeBinder;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class TenantFromAuthenticationKey : ITenantFinder
	{
		private readonly TenantLoader _tenants;

		public TenantFromAuthenticationKey(TenantLoader tenants)
		{
			_tenants = tenants;
		}

		public string Find(object argument)
		{
			dynamic dynamic = argument;
			var key = tryGet(() => dynamic.AuthenticationKey);
			if (key != null)
				return _tenants.TenantNameByKey(LegacyAuthenticationKey.MakeEncodingSafe(key));
			return null;
		}
		
		private static string tryGet(Func<string> input)
		{
			try
			{
				return input();
			}
			catch (RuntimeBinderException) { }
			return null;
		}
		
	}
}
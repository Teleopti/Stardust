using System.Configuration;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Sdk.WcfService.Factory;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
{
	public static class TenancyLogonFactory
	{
		static IMultiTenancyApplicationLogon _multiTenancyApplicationLogon;
		private static IMultiTenancyWindowsLogon _multiTenancyWindowsLogon;

		public static IMultiTenancyApplicationLogon MultiTenancyApplicationLogon()
		{
			if (_multiTenancyApplicationLogon == null)
			{
				_multiTenancyApplicationLogon = new MultiTenancyApplicationLogon(
					new AuthenticationQuerier(new TenantServerConfiguration(ConfigurationManager.AppSettings["TenantServer"]), new NhibConfigDecryption(), new PostHttpRequest(), new NewtonsoftJsonSerializer(),() => StateHolder.Instance.StateReader.ApplicationScopeData, new VerifyTerminalDate(() => StateHolder.Instance.StateReader.ApplicationScopeData)),
					() => StateHolder.Instance.StateReader.ApplicationScopeData,
					new LoadUserUnauthorized());
			}
			return _multiTenancyApplicationLogon;
		}

		public static IMultiTenancyWindowsLogon MultiTenancyWindowsLogon()
		{
			if (_multiTenancyWindowsLogon == null)
			{
				_multiTenancyWindowsLogon = new MultiTenancyWindowsLogon(
					new AuthenticationQuerier(new TenantServerConfiguration(ConfigurationManager.AppSettings["TenantServer"]), new NhibConfigDecryption(), new PostHttpRequest(), new NewtonsoftJsonSerializer(), () => StateHolder.Instance.StateReader.ApplicationScopeData, new VerifyTerminalDate(() => StateHolder.Instance.StateReader.ApplicationScopeData)),
					new WebWindowsUserProvider(), 
					() => StateHolder.Instance.StateReader.ApplicationScopeData,
					new LoadUserUnauthorized());
			}
			return _multiTenancyWindowsLogon;
		}
	}
}
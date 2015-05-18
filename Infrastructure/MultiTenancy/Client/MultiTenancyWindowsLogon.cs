using System;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class MultiTenancyWindowsLogon : IMultiTenancyWindowsLogon
	{
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;
		private readonly Func<IApplicationData> _applicationData;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;

		public MultiTenancyWindowsLogon(IAuthenticationQuerier authenticationQuerier,
			IWindowsUserProvider windowsUserProvider, Func<IApplicationData> applicationData, ILoadUserUnauthorized loadUserUnauthorized) 
		{
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
			_applicationData = applicationData;
			_loadUserUnauthorized = loadUserUnauthorized;
		}

		public AuthenticationResult Logon(string userAgent)
		{
			var identity = _windowsUserProvider.Identity();
			var result = _authenticationQuerier.TryLogon(new IdentityLogonClientModel{Identity = identity}, userAgent);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = result.FailReason
				};

			var datasource = _applicationData().Tenant(result.Tenant);
			return new AuthenticationResult
			{
				Person = _loadUserUnauthorized.LoadFullPersonInSeperateTransaction(datasource.Application, result.PersonId),
				Successful = true,
				DataSource = datasource
			};
		}

		public bool CheckWindowsIsPossible()
		{
			return _authenticationQuerier.TryLogon(new IdentityLogonClientModel{Identity = _windowsUserProvider.Identity()}, "").Success;
		}
	}
}
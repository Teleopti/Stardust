using System;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class MultiTenancyApplicationLogon : IMultiTenancyApplicationLogon
	{
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly Func<IApplicationData> _applicationData;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;

		public MultiTenancyApplicationLogon(IAuthenticationQuerier authenticationQuerier, 
																		Func<IApplicationData> applicationData,
																		ILoadUserUnauthorized loadUserUnauthorized)
		{
			_authenticationQuerier = authenticationQuerier;
			_applicationData = applicationData;
			_loadUserUnauthorized = loadUserUnauthorized;
		}

		public AuthenticationResult Logon(string userName, string password, string userAgent)
		{
			var result = _authenticationQuerier.TryLogon(new ApplicationLogonClientModel{UserName = userName, Password = password}, userAgent);
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
	}
}
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

		public AuthenticationResult Logon(LogonModel logonModel, string userAgent)
		{
			var result = _authenticationQuerier.TryLogon(new ApplicationLogonClientModel{UserName = logonModel.UserName, Password = logonModel.Password}, userAgent);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = result.FailReason
				};

			var dataSourceName = result.Tenant;
			var personId = result.PersonId;
			
			logonModel.SelectedDataSourceContainer = new DataSourceContainer(_applicationData().Tenant(dataSourceName), AuthenticationTypeOption.Application);

			var person = _loadUserUnauthorized.LoadFullPersonInSeperateTransaction(logonModel.SelectedDataSourceContainer.DataSource.Application, personId);
			logonModel.SelectedDataSourceContainer.SetUser(person);

			return new AuthenticationResult
			{
				Person = logonModel.SelectedDataSourceContainer.User,
				Successful = true
			};
		}
	}
}
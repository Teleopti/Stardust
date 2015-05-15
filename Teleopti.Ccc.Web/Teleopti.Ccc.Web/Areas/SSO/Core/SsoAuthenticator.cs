using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public class SsoAuthenticator : ISsoAuthenticator
	{
		private readonly IApplicationData _applicationData;
		private readonly IApplicationAuthentication _applicationAuthentication;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;

		public SsoAuthenticator(IApplicationData applicationData,
																IApplicationAuthentication applicationAuthentication,
																ILoadUserUnauthorized loadUserUnauthorized)
		{
			_applicationData = applicationData;
			_applicationAuthentication = applicationAuthentication;
			_loadUserUnauthorized = loadUserUnauthorized;
		}

		public AuthenticateResult AuthenticateApplicationUser(string userName, string password)
		{
			var result = _applicationAuthentication.Logon(userName, password);

			if (result.Success)
			{
				var dataSource = _applicationData.Tenant(result.Tenant);
				var person = _loadUserUnauthorized.LoadFullPersonInSeperateTransaction(dataSource.Application, result.PersonId); //TODO tenant: - don't load permissions here - just needed to get web scenarios to work
				return new AuthenticateResult { DataSource = dataSource, Person = person, Successful = true, HasMessage = !string.IsNullOrEmpty(result.FailReason), Message = result.FailReason, PasswordExpired = false };
			}

			return new AuthenticateResult { DataSource = null, Person = null, Successful = false, HasMessage = !string.IsNullOrEmpty(result.FailReason), Message = result.FailReason, PasswordExpired = result.PasswordExpired };
		}
	}
}
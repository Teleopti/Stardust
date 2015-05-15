using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IIdentityLogon
	{
		private readonly IApplicationData _applicationData;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IFindTenantAndPersonIdForIdentity _findTenantAndPersonIdForIdentity;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;

		public Authenticator(IApplicationData applicationData,
									ITokenIdentityProvider tokenIdentityProvider,
									IFindTenantAndPersonIdForIdentity findTenantAndPersonIdForIdentity,
									ILoadUserUnauthorized loadUserUnauthorized)
		{
			_applicationData = applicationData;
			_tokenIdentityProvider = tokenIdentityProvider;
			_findTenantAndPersonIdForIdentity = findTenantAndPersonIdForIdentity;
			_loadUserUnauthorized = loadUserUnauthorized;
		}

		public AuthenticateResult LogonIdentityUser()
		{
			var token = _tokenIdentityProvider.RetrieveToken();
			var personInfo = _findTenantAndPersonIdForIdentity.Find(token.UserIdentifier);
			if (personInfo == null)
				return new AuthenticateResult { Successful = false };

			var dataSource = _applicationData.Tenant(personInfo.Tenant);
			var foundAppUser = _loadUserUnauthorized.LoadFullPersonInSeperateTransaction(dataSource.Application, personInfo.PersonId);
			return foundAppUser.IsTerminated() ? 
				new AuthenticateResult { Successful = false } : 
				new AuthenticateResult { Successful = true, Person = foundAppUser, DataSource = dataSource };
		}
	}
}
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
		private readonly IFindPersonInfoByIdentity _findPersonInfoByIdentity;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;

		public Authenticator(IApplicationData applicationData,
									ITokenIdentityProvider tokenIdentityProvider,
									IFindPersonInfoByIdentity findPersonInfoByIdentity,
									ILoadUserUnauthorized loadUserUnauthorized)
		{
			_applicationData = applicationData;
			_tokenIdentityProvider = tokenIdentityProvider;
			_findPersonInfoByIdentity = findPersonInfoByIdentity;
			_loadUserUnauthorized = loadUserUnauthorized;
		}

		public AuthenticatorResult LogonIdentityUser()
		{
			var token = _tokenIdentityProvider.RetrieveToken();
			var personInfo = _findPersonInfoByIdentity.Find(token.UserIdentifier);
			if (personInfo == null)
				return new AuthenticatorResult { Successful = false };

			var dataSource = _applicationData.Tenant(personInfo.Tenant.Name);
			var foundAppUser = _loadUserUnauthorized.LoadFullPersonInSeperateTransaction(dataSource.Application, personInfo.Id);
			return foundAppUser.IsTerminated() ?
				new AuthenticatorResult { Successful = false } :
				new AuthenticatorResult { Successful = true, Person = foundAppUser, DataSource = dataSource, TenantPassword = personInfo.TenantPassword};
		}
	}
}
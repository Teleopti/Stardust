using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IIdentityLogon
	{
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IFindPersonInfoByIdentity _findPersonInfoByIdentity;
		private readonly ILoadUserUnauthorized _loadUserUnauthorized;

		public Authenticator(IDataSourceForTenant dataSourceForTenant,
									ITokenIdentityProvider tokenIdentityProvider,
									IFindPersonInfoByIdentity findPersonInfoByIdentity,
									ILoadUserUnauthorized loadUserUnauthorized)
		{
			_dataSourceForTenant = dataSourceForTenant;
			_tokenIdentityProvider = tokenIdentityProvider;
			_findPersonInfoByIdentity = findPersonInfoByIdentity;
			_loadUserUnauthorized = loadUserUnauthorized;
		}

		public AuthenticatorResult LogonIdentityUser()
		{
			var token = _tokenIdentityProvider.RetrieveToken();
			if(token == null)
				return new AuthenticatorResult { Successful = false };
			var personInfo = _findPersonInfoByIdentity.Find(token.UserIdentifier, token.IsTeleoptiApplicationLogon);
			if (personInfo == null)
				return new AuthenticatorResult { Successful = false };

			var dataSource = _dataSourceForTenant.Tenant(personInfo.Tenant.Name);
			var foundAppUser = _loadUserUnauthorized.LoadFullPersonInSeperateTransaction(dataSource.Application, personInfo.Id);
			return foundAppUser.IsTerminated() ?
				new AuthenticatorResult { Successful = false } :
				new AuthenticatorResult { Successful = true, Person = foundAppUser, DataSource = dataSource, TenantPassword = personInfo.TenantPassword, IsPersistent = token.IsPersistent};
		}
	}
}
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class IdentityAuthentication : IIdentityAuthentication
	{
		private readonly IIdentityUserQuery _identityUserQuery;
		private readonly IDataSourceConfigurationProvider _dataSourceConfigurationProvider;

		public IdentityAuthentication(IIdentityUserQuery identityUserQuery,
			IDataSourceConfigurationProvider dataSourceConfigurationProvider)
		{
			_identityUserQuery = identityUserQuery;
			_dataSourceConfigurationProvider = dataSourceConfigurationProvider;
		}

		public TenantAuthenticationResult Logon(string identity)
		{
			var foundUser = _identityUserQuery.FindUserData(identity);
			if (foundUser==null)
				return createFailingResult(string.Format(Resources.LogOnFailedIdentityNotFound, identity));

			var nhibConfig = _dataSourceConfigurationProvider.ForTenant(foundUser.Tenant);
			//TODO tenant: no need to keep this when #33685 is done
			if (nhibConfig==null)
				return createFailingResult(Resources.NoDatasource);
			
			return new TenantAuthenticationResult
			{
				Success = true,
				PersonId = foundUser.Id,
				Tenant = foundUser.Tenant.Name,
				DataSourceConfiguration = nhibConfig,
				TenantPassword = foundUser.TenantPassword
			};
		}

		private static TenantAuthenticationResult createFailingResult(string failReason)
		{
			return new TenantAuthenticationResult
			{
				Success = false,
				FailReason = failReason
			};
		}
	}
}
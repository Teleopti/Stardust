using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
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

		public ApplicationAuthenticationResult Logon(string identity)
		{
			var foundUser = _identityUserQuery.FindUserData(identity);
			if (foundUser==null)
				return createFailingResult(string.Format(Resources.LogOnFailedIdentityNotFound, identity));

			var nhibConfig = _dataSourceConfigurationProvider.ForTenant(foundUser.Tenant);
			if (nhibConfig==null)
				return createFailingResult(Resources.NoDatasource);
			
			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = foundUser.Id,
				Tenant = foundUser.Tenant,
				DataSourceConfiguration = nhibConfig
			};
		}

		private static ApplicationAuthenticationResult createFailingResult(string failReason)
		{
			return new ApplicationAuthenticationResult
			{
				Success = false,
				FailReason = failReason
			};
		}
	}
}
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class IdentityAuthentication : IIdentityAuthentication
	{
		private readonly IIdentityUserQuery _identityUserQuery;
		private readonly IDataSourceConfigurationProvider _dataSourceConfigurationProvider;
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;

		public IdentityAuthentication(IIdentityUserQuery identityUserQuery,
			IDataSourceConfigurationProvider dataSourceConfigurationProvider,
			ILoadPasswordPolicyService loadPasswordPolicyService)
		{
			_identityUserQuery = identityUserQuery;
			_dataSourceConfigurationProvider = dataSourceConfigurationProvider;
			_loadPasswordPolicyService = loadPasswordPolicyService;
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
				DataSourceConfiguration = nhibConfig,
				PasswordPolicy = _loadPasswordPolicyService.DocumentAsString
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
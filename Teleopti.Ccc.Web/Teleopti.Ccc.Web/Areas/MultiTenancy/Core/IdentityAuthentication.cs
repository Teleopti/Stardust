using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class IdentityAuthentication : IIdentityAuthentication
	{
		private readonly IIdentityUserQuery _identityUserQuery;
		private readonly IDataSourceConfigurationEncryption _dataSourceConfigurationEncryption;

		public IdentityAuthentication(IIdentityUserQuery identityUserQuery,
																	IDataSourceConfigurationEncryption dataSourceConfigurationEncryption)
		{
			_identityUserQuery = identityUserQuery;
			_dataSourceConfigurationEncryption = dataSourceConfigurationEncryption;
		}

		public TenantAuthenticationResult Logon(string identity)
		{
			var foundUser = _identityUserQuery.FindUserData(identity);
			if (foundUser==null)
				return createFailingResult(string.Format(Resources.LogOnFailedIdentityNotFound, identity));
			
			return new TenantAuthenticationResult
			{
				Success = true,
				PersonId = foundUser.Id,
				Tenant = foundUser.Tenant.Name,
				DataSourceConfiguration = _dataSourceConfigurationEncryption.EncryptConfig(foundUser.Tenant.DataSourceConfiguration),
				ApplicationConfig = foundUser.Tenant.ApplicationConfig,
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
using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class IdAuthentication : IIdAuthentication
	{
		private readonly IIdUserQuery _idUserQuery;
		private readonly IDataSourceConfigurationEncryption _dataSourceConfigurationEncryption;

		public IdAuthentication(IIdUserQuery idUserQuery,
																	IDataSourceConfigurationEncryption dataSourceConfigurationEncryption)
		{
			_idUserQuery = idUserQuery;
			_dataSourceConfigurationEncryption = dataSourceConfigurationEncryption;
		}

		public TenantAuthenticationResult Logon(Guid id)
		{
			var foundUser = _idUserQuery.FindUserData(id);
			if (foundUser==null)
				return createFailingResult(string.Format(Resources.LogOnFailedIdentityNotFound, id));
			
			return new TenantAuthenticationResult
			{
				Success = true,
				PersonId = foundUser.Id,
				Tenant = foundUser.Tenant.Name,
				DataSourceConfiguration = _dataSourceConfigurationEncryption.EncryptConfig(foundUser.Tenant.DataSourceConfiguration),
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
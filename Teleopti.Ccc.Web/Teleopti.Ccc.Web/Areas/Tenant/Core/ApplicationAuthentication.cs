using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		private readonly IApplicationUserTenantQuery _applicationUserQuery;
		private readonly IDataSourceConfigurationProvider _dataSourceConfigurationProvider;
		private readonly Func<IPasswordPolicy> _passwordPolicy;
		private readonly INow _now;
		private readonly ITenantCheckPasswordChange _tenantCheckPasswordChange;

		public ApplicationAuthentication(IApplicationUserTenantQuery applicationUserQuery,
			IDataSourceConfigurationProvider dataSourceConfigurationProvider,
			Func<IPasswordPolicy> passwordPolicy, INow now, ITenantCheckPasswordChange tenantCheckPasswordChange)
		{
			_applicationUserQuery = applicationUserQuery;
			_dataSourceConfigurationProvider = dataSourceConfigurationProvider;
			_passwordPolicy = passwordPolicy;
			_now = now;
			_tenantCheckPasswordChange = tenantCheckPasswordChange;
		}

		public ApplicationAuthenticationResult Logon(string userName, string password)
		{
			var personInfo = _applicationUserQuery.Find(userName);
			//TODO: add check!
			if (personInfo == null)
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			var applicationLogonInfo = personInfo.ApplicationLogonInfo;

			if(!applicationLogonInfo.IsValidPassword(_now, _passwordPolicy(), password))
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (applicationLogonInfo.IsLocked)
				return createFailingResult(Resources.LogOnFailedAccountIsLocked);

			var nhibConfig = _dataSourceConfigurationProvider.ForTenant(applicationLogonInfo.PersonInfo.Tenant);
			if (nhibConfig == null)
				return createFailingResult(Resources.NoDatasource);

			var passwordCheck = _tenantCheckPasswordChange.Check(applicationLogonInfo);
			if (passwordCheck != null)
				return new ApplicationAuthenticationResult
				{
					FailReason = passwordCheck.Message,
					PasswordExpired = passwordCheck.PasswordExpired,
					Success = passwordCheck.Successful,
					Tenant = applicationLogonInfo.PersonInfo.Tenant
				};

			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = applicationLogonInfo.PersonInfo.Id,
				Tenant = applicationLogonInfo.PersonInfo.Tenant,
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
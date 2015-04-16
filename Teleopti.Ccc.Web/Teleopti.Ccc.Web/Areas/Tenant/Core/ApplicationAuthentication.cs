using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		private readonly IApplicationUserTenantQuery _applicationUserQuery;
		private readonly IPasswordVerifier _passwordVerifier;
		private readonly IPasswordPolicyCheck _passwordPolicyCheck;
		private readonly IDataSourceConfigurationProvider _dataSourceConfigurationProvider;

		public ApplicationAuthentication(IApplicationUserTenantQuery applicationUserQuery, IPasswordVerifier passwordVerifier,
			IPasswordPolicyCheck passwordPolicyCheck, IDataSourceConfigurationProvider dataSourceConfigurationProvider)
		{
			_applicationUserQuery = applicationUserQuery;
			_passwordVerifier = passwordVerifier;
			_passwordPolicyCheck = passwordPolicyCheck;
			_dataSourceConfigurationProvider = dataSourceConfigurationProvider;
		}

		public ApplicationAuthenticationResult Logon(string userName, string password)
		{
			var personInfo = _applicationUserQuery.Find(userName);
			//TODO: add check!
			if (personInfo == null)
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			var applicationLogonInfo = personInfo.ApplicationLogonInfo;

			if (!_passwordVerifier.Check(password, applicationLogonInfo))
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (applicationLogonInfo.IsLocked)
				return createFailingResult(Resources.LogOnFailedAccountIsLocked);

			var nhibConfig = _dataSourceConfigurationProvider.ForTenant(applicationLogonInfo.PersonInfo.Tenant);
			if (nhibConfig == null)
				return createFailingResult(Resources.NoDatasource);

			var passwordCheck = _passwordPolicyCheck.Verify(applicationLogonInfo);
			if (passwordCheck != null)
				return passwordCheck;

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
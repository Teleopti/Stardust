using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		private readonly IApplicationUserQuery _applicationUserQuery;
		private readonly IPasswordVerifier _passwordVerifier;
		private readonly IPasswordPolicyCheck _passwordPolicyCheck;
		private readonly IDataSourceConfigurationProvider _dataSourceConfigurationProvider;
		
		public ApplicationAuthentication(IApplicationUserQuery applicationUserQuery, IPasswordVerifier passwordVerifier,
			IPasswordPolicyCheck passwordPolicyCheck, IDataSourceConfigurationProvider dataSourceConfigurationProvider)
		{
			_applicationUserQuery = applicationUserQuery;
			_passwordVerifier = passwordVerifier;
			_passwordPolicyCheck = passwordPolicyCheck;
			_dataSourceConfigurationProvider = dataSourceConfigurationProvider;
	}

		public ApplicationAuthenticationResult Logon(string userName, string password)
		{
			var passwordPolicyForUser = _applicationUserQuery.FindUserData(userName);
			if (passwordPolicyForUser == null)
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (!_passwordVerifier.Check(password, passwordPolicyForUser))
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (passwordPolicyForUser.IsLocked)
				return createFailingResult(Resources.LogOnFailedAccountIsLocked);

			var nhibConfig = _dataSourceConfigurationProvider.ForTenant(passwordPolicyForUser.PersonInfo.Tenant);
			if (nhibConfig == null)
				return createFailingResult(Resources.NoDatasource);

			var passwordCheck = _passwordPolicyCheck.Verify(passwordPolicyForUser);
			if (passwordCheck != null)
				return passwordCheck;

			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = passwordPolicyForUser.PersonInfo.Id,
				Tenant = passwordPolicyForUser.PersonInfo.Tenant,
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
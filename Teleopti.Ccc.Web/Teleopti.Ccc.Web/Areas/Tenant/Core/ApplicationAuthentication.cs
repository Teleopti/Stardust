using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		private readonly IApplicationUserQuery _applicationUserQuery;
		private readonly IPasswordVerifier _passwordVerifier;
		private readonly IPasswordPolicyCheck _passwordPolicyCheck;
		private readonly INHibernateConfigurationsHandler _nHibernateConfigurationsHandler;
		
		public ApplicationAuthentication(IApplicationUserQuery applicationUserQuery, IPasswordVerifier passwordVerifier,
			IPasswordPolicyCheck passwordPolicyCheck, INHibernateConfigurationsHandler nHibernateConfigurationsHandler)
		{
			_applicationUserQuery = applicationUserQuery;
			_passwordVerifier = passwordVerifier;
			_passwordPolicyCheck = passwordPolicyCheck;
			_nHibernateConfigurationsHandler = nHibernateConfigurationsHandler;
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

			var passwordCheck = _passwordPolicyCheck.Verify(passwordPolicyForUser);
			if (passwordCheck != null)
				return passwordCheck;

			var nhibConfig = _nHibernateConfigurationsHandler.GetConfigForName(passwordPolicyForUser.PersonInfo.Tennant);
			if(string.IsNullOrEmpty(nhibConfig))
				return createFailingResult(Resources.NoDatasource);

			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = passwordPolicyForUser.PersonInfo.Id,
				Tennant = passwordPolicyForUser.PersonInfo.Tennant,
				DataSourceConfig = nhibConfig
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
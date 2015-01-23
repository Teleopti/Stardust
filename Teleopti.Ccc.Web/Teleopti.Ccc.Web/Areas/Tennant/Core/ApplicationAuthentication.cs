using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		private readonly IApplicationUserQuery _applicationUserQuery;
		private readonly IPasswordVerifier _passwordVerifier;
		private readonly IPasswordPolicyCheck _passwordPolicyCheck;

		public ApplicationAuthentication(IApplicationUserQuery applicationUserQuery, IPasswordVerifier passwordVerifier, IPasswordPolicyCheck passwordPolicyCheck)
		{
			_applicationUserQuery = applicationUserQuery;
			_passwordVerifier = passwordVerifier;
			_passwordPolicyCheck = passwordPolicyCheck;
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

			string passwordPolicyFailureReason;
			if (!_passwordPolicyCheck.Verify(passwordPolicyForUser, out passwordPolicyFailureReason))
				return createFailingResult(passwordPolicyFailureReason);
	

			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = passwordPolicyForUser.PersonInfo.Id,
				Tennant = passwordPolicyForUser.PersonInfo.Tennant
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
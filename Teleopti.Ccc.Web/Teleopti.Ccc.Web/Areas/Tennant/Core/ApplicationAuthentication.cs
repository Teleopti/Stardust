using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
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
			var foundUser = _applicationUserQuery.FindUserData(userName);
			if (foundUser == null)
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (!_passwordVerifier.Check(password, foundUser.PersonInfo.Password))
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (foundUser.PasswordPolicy.IsLocked)
				return createFailingResult(Resources.LogOnFailedAccountIsLocked);

			string passwordPolicyFailureReason;
			if (!_passwordPolicyCheck.Verify(foundUser.PasswordPolicy.InvalidAttempts, foundUser.PasswordPolicy.InvalidAttemptsSequenceStart, foundUser.PasswordPolicy.LastPasswordChange, out passwordPolicyFailureReason))
				return createFailingResult(passwordPolicyFailureReason);
	

			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = foundUser.PersonInfo.Id,
				Tennant = foundUser.PersonInfo.Tennant
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
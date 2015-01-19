using Teleopti.Ccc.Domain.Security;
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
			var foundUser = _applicationUserQuery.FindUserData(userName);
			if (foundUser == null)
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (!_passwordVerifier.Check(password, foundUser.Password))
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (foundUser.IsLocked)
				return createFailingResult(Resources.LogOnFailedAccountIsLocked);

			string passwordPolicyFailureReason;
			if (!_passwordPolicyCheck.Verify(foundUser.InvalidAttempts, foundUser.InvalidAttemptsSequenceStart, foundUser.LastPasswordChange, out passwordPolicyFailureReason))
				return createFailingResult(passwordPolicyFailureReason);
	

			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = foundUser.PersonId,
				Tennant = foundUser.Tennant
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
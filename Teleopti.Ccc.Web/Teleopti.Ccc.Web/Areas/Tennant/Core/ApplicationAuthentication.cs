using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class ApplicationAuthentication : IApplicationAuthentication
	{
		private readonly IApplicationUserQuery _applicationUserQuery;
		private readonly IOneWayEncryption _oneWayEncryption;

		public ApplicationAuthentication(IApplicationUserQuery applicationUserQuery, IOneWayEncryption oneWayEncryption)
		{
			_applicationUserQuery = applicationUserQuery;
			_oneWayEncryption = oneWayEncryption;
		}

		public ApplicationAuthenticationResult Logon(string userName, string password)
		{
			var foundUser = _applicationUserQuery.FindUserData(userName);
			if (foundUser == null)
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);

			if (foundUser.Password != _oneWayEncryption.EncryptString(password))
				return createFailingResult(Resources.LogOnFailedInvalidUserNameOrPassword);


			/////move away - continue later
			if (foundUser.IsLocked)
				return createFailingResult(Resources.LogOnFailedAccountIsLocked);

			/////

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
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	class IdentityAuthentication : IIdentityAuthentication
	{
		private readonly IIdentityUserQuery _identityUserQuery;

		public IdentityAuthentication(IIdentityUserQuery identityUserQuery)
		{
			_identityUserQuery = identityUserQuery;
		}

		public ApplicationAuthenticationResult Logon(string identity)
		{
			var foundUser = _identityUserQuery.FindUserData(identity);
			if (!foundUser.Success)
				return createFailingResult(string.Format(Resources.LogOnFailedIdentityNotFound, identity));

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
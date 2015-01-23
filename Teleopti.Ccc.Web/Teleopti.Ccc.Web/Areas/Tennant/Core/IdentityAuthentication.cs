using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class IdentityAuthentication : IIdentityAuthentication
	{
		private readonly IIdentityUserQuery _identityUserQuery;
		private readonly INHibernateConfigurationsHandler _nHibernateConfigurationsHandler;
		
		public IdentityAuthentication(IIdentityUserQuery identityUserQuery, INHibernateConfigurationsHandler nHibernateConfigurationsHandler)
		{
			_identityUserQuery = identityUserQuery;
			_nHibernateConfigurationsHandler = nHibernateConfigurationsHandler;
		}

		public ApplicationAuthenticationResult Logon(string identity)
		{
			var foundUser = _identityUserQuery.FindUserData(identity);
			if (foundUser==null)
				return createFailingResult(string.Format(Resources.LogOnFailedIdentityNotFound, identity));

			string encryptedString = _nHibernateConfigurationsHandler.GetConfigForName(foundUser.Tennant);
			if (string.IsNullOrEmpty(encryptedString))
				return createFailingResult(Resources.NoDatasource);
			
			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = foundUser.Id,
				Tennant = foundUser.Tennant,
				DataSourceEncrypted = encryptedString
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
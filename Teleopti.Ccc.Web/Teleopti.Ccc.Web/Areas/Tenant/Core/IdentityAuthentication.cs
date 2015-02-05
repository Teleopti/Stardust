using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
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

			var nhibConfig = _nHibernateConfigurationsHandler.GetConfigForName(foundUser.Tenant);
			if (string.IsNullOrEmpty(nhibConfig))
				return createFailingResult(Resources.NoDatasource);
			
			return new ApplicationAuthenticationResult
			{
				Success = true,
				PersonId = foundUser.Id,
				Tenant = foundUser.Tenant,
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
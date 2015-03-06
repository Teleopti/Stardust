namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindTenantAndPersonIdForIdentity : IFindTenantAndPersonIdForIdentity
	{
		private readonly IIdentityUserQuery _identityUserQuery;
		private readonly IApplicationUserTenantQuery _applicationUserTenantQuery;

		public FindTenantAndPersonIdForIdentity(IIdentityUserQuery identityUserQuery, IApplicationUserTenantQuery applicationUserTenantQuery)
		{
			_identityUserQuery = identityUserQuery;
			_applicationUserTenantQuery = applicationUserTenantQuery;
		}

		public TenantAndPersonId Find(string identity)
		{
			var identityHit = _identityUserQuery.FindUserData(identity);
			if (identityHit != null)
				return new TenantAndPersonId {PersonId = identityHit.Id, Tenant = identityHit.Tenant};

			var appHit = _applicationUserTenantQuery.Find(identity);
			if (appHit != null)
				return new TenantAndPersonId {PersonId = appHit.Id, Tenant = appHit.Tenant};

			return null;
		}
	}
}
namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindTenantAndPersonIdForIdentity : IFindTenantAndPersonIdForIdentity
	{
		private readonly IIdentityUserQuery _identityUserQuery;
		private readonly IApplicationUserQuery _applicationUserQuery;

		public FindTenantAndPersonIdForIdentity(IIdentityUserQuery identityUserQuery, IApplicationUserQuery applicationUserQuery)
		{
			_identityUserQuery = identityUserQuery;
			_applicationUserQuery = applicationUserQuery;
		}

		public TenantAndPersonId Find(string identity)
		{
			var identityHit = _identityUserQuery.FindUserData(identity);
			if (identityHit != null)
				return new TenantAndPersonId {PersonId = identityHit.Id, Tenant = identityHit.Tenant};

			var appHit = _applicationUserQuery.Find(identity);
			if (appHit != null)
				return new TenantAndPersonId {PersonId = appHit.Id, Tenant = appHit.Tenant};

			return null;
		}
	}
}
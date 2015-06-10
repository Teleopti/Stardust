namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class FindPersonInfoByIdentity : IFindPersonInfoByIdentity
	{
		private readonly IIdentityUserQuery _identityUserQuery;
		private readonly IApplicationUserQuery _applicationUserQuery;

		public FindPersonInfoByIdentity(IIdentityUserQuery identityUserQuery, IApplicationUserQuery applicationUserQuery)
		{
			_identityUserQuery = identityUserQuery;
			_applicationUserQuery = applicationUserQuery;
		}

		public PersonInfo Find(string identity)
		{
			var identityHit = _identityUserQuery.FindUserData(identity);
			return identityHit ?? _applicationUserQuery.Find(identity);
		}
	}
}
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

		public PersonInfo Find(string identity, bool isTeleoptiApplicationLogon)
		{
			return isTeleoptiApplicationLogon ? _applicationUserQuery.Find(identity) : _identityUserQuery.FindUserData(identity);
		}
	}
}
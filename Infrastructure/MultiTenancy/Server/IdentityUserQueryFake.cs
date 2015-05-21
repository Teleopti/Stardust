namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class IdentityUserQueryFake : IIdentityUserQuery
	{
		public PersonInfo FindUserData(string identity)
		{
			return null;
		}
	}
}
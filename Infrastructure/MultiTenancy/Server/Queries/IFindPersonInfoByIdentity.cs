namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public interface IFindPersonInfoByIdentity
	{
		PersonInfo Find(string identity, bool isTeleoptiApplicationLogon);
	}
}
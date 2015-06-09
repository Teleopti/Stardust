namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IFindPersonInfoByIdentity
	{
		PersonInfo Find(string identity);
	}
}
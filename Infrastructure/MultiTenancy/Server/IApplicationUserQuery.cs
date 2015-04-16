namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public interface IApplicationUserQuery
	{
		ApplicationLogonInfo FindUserData(string userName);
	}
}
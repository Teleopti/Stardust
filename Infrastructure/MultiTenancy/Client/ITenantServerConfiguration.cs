namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface ITenantServerConfiguration
	{
		string FullPath(string relativeUrl);
	}
}
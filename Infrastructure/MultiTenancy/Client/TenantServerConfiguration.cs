namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantServerConfiguration : ITenantServerConfiguration
	{
		public TenantServerConfiguration(string pathToServer)
		{
			Path = pathToServer;
		}

		public string Path { get; private set; }
	}
}
namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class TenantServerConfiguration : ITenantServerConfiguration
	{
		private readonly string _pathToServer;

		public TenantServerConfiguration(string pathToServer)
		{
			_pathToServer = pathToServer;
		}

		public string FullPath(string relativeUrl)
		{
			return relativeUrl == string.Empty || _pathToServer.EndsWith("/")
				? _pathToServer + relativeUrl
				: _pathToServer + "/" + relativeUrl;
		}
	}
}
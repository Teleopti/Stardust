using System;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core
{
	public class SystemVersion
	{
		private Version _version;

		public void Is(string version) =>
			_version = new Version(version);

		public void Reset() =>
			_version = null;

		public string Version() => version().ToString();

		public string VersionOrTimestamp() =>
			version() == new Version(version().Major, 0, 0, 0)
				? DateTime.UtcNow.ToString("yyyyMMddHHmmssffff")
				: version().ToString();

		private Version version() =>
			_version ?? typeof(ApplicationStartModule).Assembly.GetName().Version;
	}
}
using System;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core
{
	public class ResourceVersion
	{
		private string _version;

		public void Is(string version)
		{
			_version = version;
		}
		
		public string TeapotVersion()
		{
			if (_version != null)
				return _version;
			return typeof(ApplicationStartModule).Assembly.GetName().Version.ToString();
		}
		
		public string Version()
		{
			var version = typeof(ApplicationStartModule).Assembly.GetName().Version;
			return version == new Version(version.Major, 0, 0, 0)
				       ? DateTime.UtcNow.ToString("yyyyMMddHHmmssffff")
				       : version.ToString();
		}
	}
}
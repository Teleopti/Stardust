using System;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Core
{
	public class ResourceVersion : IResourceVersion
	{
		public string Version()
		{
			var version = typeof(ApplicationStartModule).Assembly.GetName().Version;
			return version == new Version(version.Major, 0, 0, 0)
				       ? DateTime.UtcNow.ToString("yyyyMMddHHmmssffff")
				       : version.ToString();
		}
	}
}
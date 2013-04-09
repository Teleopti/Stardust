using System.IO;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class Paths
	{
		public static string WebBinPath()
		{
			var path = Path.Combine(IniFileInfo.SitePath, "bin");
			return new DirectoryInfo(path).FullName;
		}

		public static string WebNHibConfPath()
		{
			return WebBinPath();
		}
	}
}
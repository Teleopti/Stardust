using System;
using System.IO;
using System.Linq;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class Paths
	{

		private static Lazy<string> _webPath = new Lazy<string>(() =>
		{
			var path = IniFileInfo.SitePath;
			return new DirectoryInfo(path).FullName;
		});

		public static string WebPath()
		{
			return _webPath.Value;
		}

		public static string WebBinPath()
		{
			var path = Path.Combine(WebPath(), "bin");
			return new DirectoryInfo(path).FullName;
		}

		public static string NhibPath()
		{
			var path = IniFileInfo.AGENTPORTALWEB_nhibConfPath;
			return new DirectoryInfo(path).FullName;
		}

		public static string WebNHibConfPath()
		{
			return WebBinPath();
		}

		public static string WebAuthenticationBridgePath()
		{
			return FindProjectPath(@"Teleopti.Ccc.Web.AuthenticationBridge\");
		}

		public static string WebWindowsIdentityProviderPath()
		{
			return FindProjectPath(@"Teleopti.Ccc.Web.WindowsIdentityProvider\");
		}

		public static string FindProjectPath(params string[] projectPaths)
		{
			var rootPaths = new[] { IniFileInfo.AGENTPORTALWEB_nhibConfPath, AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() };
			var relativePaths = new[] { "", @"..\", @"..\..\", @"..\..\..\", @"..\..\..\..\" };
			var possiblePaths = (from root in rootPaths
								 from reletive in relativePaths
								 from projectPath in projectPaths
								 select Path.Combine(root, reletive, projectPath)
								).ToArray();
			var path = possiblePaths.FirstOrDefault(Directory.Exists);
			if (path == null)
				throw new Exception(
					"project paths " + string.Join(Environment.NewLine, projectPaths) +
					" not found. Has it been built? Tried with paths: " +
					string.Join(Environment.NewLine, possiblePaths));
			return new DirectoryInfo(path).FullName;
		}
	}
}
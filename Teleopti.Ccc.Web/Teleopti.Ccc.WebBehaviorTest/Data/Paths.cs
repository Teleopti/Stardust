using System;
using System.IO;
using System.Linq;
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

		public static string NhibPath()
		{
			var path = IniFileInfo.AGENTPORTALWEB_nhibConfPath;
			return new DirectoryInfo(path).FullName;
		}

		public static string WebNHibConfPath()
		{
			return WebBinPath();
		}

		public static string WebPath()
		{
			var path = IniFileInfo.SitePath;
			return new DirectoryInfo(path).FullName;
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
using System;
using System.IO;
using System.Linq;

namespace ManagerTest.Database
{
	public class SchemaVersionInformation
	{
		public int ReleaseNumberOfFile(FileInfo file)
		{
			var name = file.Name.Replace(".sql", "");
			var fileVersion = Convert.ToInt32(name);
			return fileVersion;
		}

		public int GetSchemaVersion(string releasesPath)
		{
			var scriptsDirectoryInfo = new DirectoryInfo(releasesPath);
			var scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);
			var versions = from f in scriptFiles select ReleaseNumberOfFile(f);
			return versions.Max();
		}
	}
}
using System;
using System.IO;
using System.Linq;

namespace Teleopti.Ccc.DBManager.Library
{
	public class SchemaVersionInformation
	{
		private readonly DatabaseFolder _databaseFolder;

		public SchemaVersionInformation(DatabaseFolder databaseFolder) {
			_databaseFolder = databaseFolder;
		}

		public int ReleaseNumberOfFile(FileInfo file)
		{
			var name = file.Name.Replace(".sql", "");
			var fileVersion = Convert.ToInt32(name);
			return fileVersion;
		}

		public int GetSchemaVersion(DatabaseType databaseType)
		{
			var releasesPath = _databaseFolder.ReleasePath(databaseType);
			var scriptsDirectoryInfo = new DirectoryInfo(releasesPath);
			var scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);
			var versions = from f in scriptFiles select ReleaseNumberOfFile(f);
			return versions.Max();
		}

		public int GetOtherScriptFilesHash(DatabaseType databaseType)
		{
			var programmabilityPath = _databaseFolder.ProgrammabilityPath(databaseType);
			var programmabilityFiles = Directory.GetFiles(programmabilityPath, "*.sql", SearchOption.AllDirectories);

			var hash = 19;
			foreach (var file in programmabilityFiles)
			{
				hash = hash * 31 + File.ReadAllText(file).GetHashCode();
			}
			return hash;
		}


	}
}
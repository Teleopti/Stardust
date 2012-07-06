using System;
using System.IO;

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

		public int TrunkVersion(DatabaseType databaseType)
		{
			var trunkPath = _databaseFolder.TrunkPath(databaseType);
			var trunkFile = Path.Combine(trunkPath, "Trunk.sql");
			var trunk = File.ReadAllText(trunkFile);
			return trunk.GetHashCode();
		}

	}
}
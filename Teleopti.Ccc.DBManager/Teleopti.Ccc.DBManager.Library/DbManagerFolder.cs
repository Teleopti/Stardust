using System;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DbManagerFolder
	{
		private readonly string _path;

		public DbManagerFolder(string path) {
			_path = path;
		}

		public DbManagerFolder() { }

		public string Path()
		{
			if (!string.IsNullOrEmpty(_path))
				return _path;

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				if (assembly.ManifestModule.Name.ToLower() == "dbmanager.exe")
				{
					return System.IO.Path.GetDirectoryName(assembly.Location);
				}
			}
			return null;
		}

		public override string ToString()
		{
			return Path();
		}
	}
}
using System;

namespace Teleopti.Support.Library.Folders
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
			return locateDatabaseFolderUsingBlackMagic();
		}

		 
		private static string locateDatabaseFolderUsingBlackMagic()
		{
			if (System.IO.Directory.Exists(@"..\..\..\..\Database"))
				return @"..\..\..\..\Database";
			if (System.IO.Directory.Exists(@"..\..\..\Database"))
				return @"..\..\..\Database";
			if (System.IO.Directory.Exists(@"..\..\Database"))
				return @"..\..\Database";
			if (System.IO.Directory.Exists(@"..\Database"))
				return @"..\Database";
			return null;
		}

		public override string ToString()
		{
			return Path();
		}
	}
}
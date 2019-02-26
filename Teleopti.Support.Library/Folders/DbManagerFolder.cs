using System;
using System.IO;
using System.Linq;

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

			return DbManagerFolderLocator.LocateDatabaseFolderUsingBlackMagic();
		}

		public override string ToString()
		{
			return Path();
		}
	}

	public static class DbManagerFolderLocator
	{
		public static string LocateDatabaseFolderUsingBlackMagic(string testDirectory = null)
		{
			var relativeLookups = new[]
			{
				@"..\..\..\..\Database",
				@"..\..\..\Database",
				@"..\..\Database",
				@"..\Database",
			};
			var lookupsFromTestDirectory = Enumerable.Empty<string>();
			if (testDirectory != null)
			{
				lookupsFromTestDirectory = relativeLookups
					.Select(x => Path.Combine(testDirectory, x))
					.ToArray();
			}

			var lookups = lookupsFromTestDirectory
				.Concat(relativeLookups)
				.ToArray();
			return lookups.FirstOrDefault(Directory.Exists);
		}
	}
}
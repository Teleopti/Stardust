using System;
using System.IO;
using System.Linq;

namespace Teleopti.Support.Library.Folders
{
	public class RepositoryRootFolder
	{
		private readonly string _path;

		public RepositoryRootFolder()
		{
			_path = RepositoryRootFolderLocator.LocateFolderUsingBlackMagic();
			if (_path != null)
				_path = System.IO.Path.GetFullPath(_path);
		}

		public string Path()
		{
			if (IsRunningFromRepository())
				return _path;
			throw new ArgumentNullException($"Not a repository!");
		}

		public bool IsRunningFromRepository() => _path != null;

		public override string ToString() => Path();
	}

	public static class RepositoryRootFolderLocator
	{
		public static string LocateFolderUsingBlackMagic(string testDirectory = null)
		{
			var relativeLookups = new[]
			{
				@"..\..\..\..\.debug-Setup",
				@"..\..\..\.debug-Setup",
				@"..\..\.debug-Setup",
				@"..\.debug-Setup",
				@".debug-Setup",
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
			
			var found = lookups.FirstOrDefault(Directory.Exists);
			
			return new FileInfo(Path.Combine(found, "..")).FullName;
		}
	}
}
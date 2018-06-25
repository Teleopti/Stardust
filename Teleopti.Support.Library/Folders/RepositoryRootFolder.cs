using System;
using System.IO;

namespace Teleopti.Support.Library.Folders
{
	public class RepositoryRootFolder
	{
		private readonly string _path;

		public RepositoryRootFolder()
		{
			_path = locateFolderUsingBlackMagic();
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

		private static string locateFolderUsingBlackMagic()
		{
			if (Directory.Exists(@"..\..\..\..\.debug-Setup"))
				return @"..\..\..\..";
			if (Directory.Exists(@"..\..\..\.debug-Setup"))
				return @"..\..\..";
			if (Directory.Exists(@"..\..\.debug-Setup"))
				return @"..\..";
			if (Directory.Exists(@"..\.debug-Setup"))
				return @"..";
			if (Directory.Exists(@".debug-Setup"))
				return @".";
			return null;
		}

		public override string ToString() => Path();
	}
}
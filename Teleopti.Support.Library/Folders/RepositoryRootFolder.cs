using System.IO;

namespace Teleopti.Support.Library.Folders
{
	public class RepositoryRootFolder
	{
		public string Path() => System.IO.Path.GetFullPath(locateFolderUsingBlackMagic());

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
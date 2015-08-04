using System.IO;

namespace Teleopti.Wfm.Administration.Core
{
	public class DbPathProviderFake : IDbPathProvider
	{
		public string GetDbPath()
		{
			return locateDatabaseFolderUsingBlackMagic(Directory.GetCurrentDirectory());
		}
		private static string locateDatabaseFolderUsingBlackMagic(string path)
		{
			if (System.IO.Directory.Exists(path + @"..\..\..\..\Database"))
				return path + @"..\..\..\..\Database";
			if (System.IO.Directory.Exists(path + @"..\..\..\Database"))
				return path + @"..\..\..\Database";
			if (System.IO.Directory.Exists(path + @"..\..\Database"))
				return path + @"..\..\Database";
			if (System.IO.Directory.Exists(path + @"..\Database"))
				return path + @"..\Database";

			//maybe works deployed (but not in Azure I guess)
			if (System.IO.Directory.Exists(path + @"..\..\..\..\DatabaseInstaller"))
				return path + @"..\..\..\..\DatabaseInstaller";
			if (System.IO.Directory.Exists(path + @"..\..\..\DatabaseInstaller"))
				return path + @"..\..\..\DatabaseInstaller";
			if (System.IO.Directory.Exists(path + @"..\..\DatabaseInstaller"))
				return path + @"..\..\DatabaseInstaller";
			if (System.IO.Directory.Exists(path + @"..\DatabaseInstaller"))
				return path + @"..\DatabaseInstaller";
			return null;
		}
	}
}
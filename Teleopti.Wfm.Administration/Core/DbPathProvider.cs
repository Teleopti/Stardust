using System.Web;

namespace Teleopti.Wfm.Administration.Core
{
	public interface IDbPathProvider
	{
		string GetDbPath();
	}
    public class DbPathProvider : IDbPathProvider
	{
		public string GetDbPath()
		{
			string path = HttpContext.Current.Server.MapPath("~/");
			return locateDatabaseFolderUsingBlackMagic(path);
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

			//azure?
			if (System.IO.Directory.Exists(path + @"..\..\..\approot\Tools\Database"))
				return path + @"..\..\..\approot\Tools\Database";
			if (System.IO.Directory.Exists(path + @"..\..\approot\Tools\Database"))
				return path + @"..\..\approot\Tools\Database";
			if (System.IO.Directory.Exists(path + @"..\approot\Tools\Database"))
				return path + @"..\approot\Tools\Database";
			return null;
		}
	}
}
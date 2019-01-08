namespace Teleopti.Ccc.DBManager.Library
{
	public class SqlVersion
	{
		public SqlVersion(int productVersion)
		{
			ProductVersion = productVersion;
		}

		public int ProductVersion { get; private set; }
	}
}
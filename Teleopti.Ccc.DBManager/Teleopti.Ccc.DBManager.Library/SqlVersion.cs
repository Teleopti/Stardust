namespace Teleopti.Ccc.DBManager.Library
{
	public class SqlVersion
	{
		public SqlVersion(int productVersion, bool isAzure)
		{
			IsAzure = isAzure;
			ProductVersion = productVersion;
		}

		public int ProductVersion { get; private set; }
		public bool IsAzure { get; private set; }
	}
}
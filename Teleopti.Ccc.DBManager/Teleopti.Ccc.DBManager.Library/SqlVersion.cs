namespace Teleopti.Ccc.DBManager.Library
{
	public struct SqlVersion
	{
		public SqlVersion(int productVersion, bool isAzure) : this()
		{
			IsAzure = isAzure;
			ProductVersion = productVersion;
		}

		public int ProductVersion { get; private set; }
		public bool IsAzure { get; private set; }
	}
}
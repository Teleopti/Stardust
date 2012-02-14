namespace Teleopti.Ccc.TestCommon
{
	public static class ConnectionStringHelper
	{
		public static string ConnectionStringUsedInTests
		{
			get { return IniFileInfo.ConnectionString; }
		}

		public static string NonValidConnectionStringUsedInTests
		{
			get { return @"Data Source=nakenjanne;Initial Catalog=Demoreg_TeleoptiCCC7;User Id=sa;Password=cadadi"; }
		}

		public static string ConnectionStringUsedInTestsMatrix
		{
			get { return IniFileInfo.ConnectionStringMatrix; }
		}

	}
}
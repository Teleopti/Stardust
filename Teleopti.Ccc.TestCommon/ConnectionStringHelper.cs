using System.Globalization;

namespace Teleopti.Ccc.TestCommon
{
	public static class ConnectionStringHelper
	{
		public static string ConnectionStringUsedInTests
		{
			get { return InfraTestConfigReader.SQL_AUTH_STRING + string.Format(CultureInfo.InvariantCulture, ";Initial Catalog={0}", InfraTestConfigReader.DB_CCC7); }
		}

		public static string NonValidConnectionStringUsedInTests
		{
			get { return @"Data Source=nakenjanne;Initial Catalog=Demoreg_TeleoptiCCC7;User Id=sa;Password=cadadi;Connect Timeout=1;"; }
		}

		public static string ConnectionStringUsedInTestsMatrix
		{
			get { return InfraTestConfigReader.SQL_AUTH_STRING + string.Format(CultureInfo.InvariantCulture, ";Initial Catalog={0}", InfraTestConfigReader.DB_ANALYTICS); }
		}

	}
}
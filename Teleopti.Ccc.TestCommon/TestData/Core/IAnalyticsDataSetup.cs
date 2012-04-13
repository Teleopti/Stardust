using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IAnalyticsDataSetup
	{
		void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture);
	}
}
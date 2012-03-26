using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces
{
	public interface IAnalyticsDataSetup
	{
		void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture);
	}
}
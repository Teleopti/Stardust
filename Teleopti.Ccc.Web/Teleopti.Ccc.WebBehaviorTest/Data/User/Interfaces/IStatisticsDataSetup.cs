using System.Data.SqlClient;
using System.Globalization;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces
{
	public interface IStatisticsDataSetup
	{
		void Apply(SqlConnection connection, CultureInfo statisticsDataCulture);
	}
}
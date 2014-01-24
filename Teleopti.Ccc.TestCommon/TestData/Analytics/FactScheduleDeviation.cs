using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FactScheduleDeviation : IAnalyticsDataSetup
	{
		private readonly int _dateId;
		private readonly int _intervalId;
		private readonly int _personId;
		private readonly int _deviationScheduleReadyS;
		private readonly bool _isLoggedIn;

		public FactScheduleDeviation(int dateId, int intervalId, int personId, int deviationScheduleReadyS, bool isLoggedIn)
		{
			_dateId = dateId;
			_intervalId = intervalId;
			_personId = personId;
			_deviationScheduleReadyS = deviationScheduleReadyS;
			_isLoggedIn = isLoggedIn;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = fact_schedule_deviation.CreateTable())
			{
				table.AddFactScheduleDeviation(_dateId, _intervalId, _personId, _deviationScheduleReadyS, _isLoggedIn);
				Bulk.Insert(connection, table);
			}
		}
	}
}
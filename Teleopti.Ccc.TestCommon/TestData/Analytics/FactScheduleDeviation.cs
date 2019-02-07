using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FactScheduleDeviation : IAnalyticsDataSetup
	{
		private readonly int _shiftStartdateLocalId;
		private readonly int _shiftStartDateId;
		private readonly int _dateId;
		private readonly int _intervalId;
		private readonly int _personId;
		private readonly int _contractTimeS;
		private readonly int _deviationScheduleS;
		private readonly int _deviationScheduleReadyS;
		private readonly int _deviationContractS;
		private readonly bool _isLoggedIn;
		private readonly int? _businessunitId;

		public FactScheduleDeviation(int shiftStartdateLocalId, int shiftStartDateId, int dateId, int intervalId, int personId, int contractTimeS, int deviationScheduleS, int deviationScheduleReadyS, int deviationContractS, bool isLoggedIn)
		{
			_shiftStartdateLocalId = shiftStartdateLocalId;
			_shiftStartDateId = shiftStartDateId;
			_dateId = dateId;
			_intervalId = intervalId;
			_personId = personId;
			_contractTimeS = contractTimeS;
			_deviationScheduleS = deviationScheduleS;
			_deviationScheduleReadyS = deviationScheduleReadyS;
			_deviationContractS = deviationContractS;
			_isLoggedIn = isLoggedIn;
		}

		public FactScheduleDeviation(int shiftStartdateLocalId, int shiftStartDateId, int dateId, int intervalId, int personId, int contractTimeS, int deviationScheduleS, int deviationScheduleReadyS, int deviationContractS, bool isLoggedIn, int businessunitId)
		{
			_shiftStartdateLocalId = shiftStartdateLocalId;
			_shiftStartDateId = shiftStartDateId;
			_dateId = dateId;
			_intervalId = intervalId;
			_personId = personId;
			_contractTimeS = contractTimeS;
			_deviationScheduleS = deviationScheduleS;
			_deviationScheduleReadyS = deviationScheduleReadyS;
			_deviationContractS = deviationContractS;
			_isLoggedIn = isLoggedIn;
			_businessunitId = businessunitId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = fact_schedule_deviation.CreateTable())
			{
				table.AddFactScheduleDeviation(_shiftStartdateLocalId, _shiftStartDateId, _dateId, _intervalId, _personId, _contractTimeS, _deviationScheduleS, _deviationScheduleReadyS, _deviationContractS, _isLoggedIn, _businessunitId);
				Bulk.Insert(connection, table);
			}
		}
	}
}
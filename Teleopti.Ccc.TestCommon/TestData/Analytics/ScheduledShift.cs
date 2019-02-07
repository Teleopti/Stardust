using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class ScheduledShift : IAnalyticsDataSetup
	{
		private readonly int _personId;
		private readonly int _shiftStartLocalDateId;
		private readonly int _scheduleDateId;
		private readonly int _shiftStartIntervalId;
		private readonly int _shiftEndIntervalId;
		private readonly int _scenarioId;
		private readonly IDatasourceData _dataSource;
		private readonly int _businessUnitId;

		public ScheduledShift(int personId, int shiftStartLocalDateId, int scheduleDateId, int shiftStartIntervalId, int shiftEndIntervalId,
			int scenarioId, IDatasourceData dataSource, int businessUnitId)
		{
			_personId = personId;
			_shiftStartLocalDateId = shiftStartLocalDateId;
			_scheduleDateId = scheduleDateId;
			_shiftStartIntervalId = shiftStartIntervalId;
			_shiftEndIntervalId = shiftEndIntervalId;
			_scenarioId = scenarioId;
			_dataSource = dataSource;
			_businessUnitId = businessUnitId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var shiftCategory = new ShiftCategory(1, Guid.NewGuid(), "sc", Color.Black, _dataSource, _businessUnitId);
			var shiftLength = new ShiftLength(1, (_shiftEndIntervalId - _shiftStartIntervalId) + 1, _dataSource);
			var absence = new Absence(1, Guid.NewGuid(), "abs", Color.Black, _dataSource, _businessUnitId);
			var activity = new Activity(1, Guid.NewGuid(), "act", Color.Black, _dataSource, _businessUnitId);

			shiftCategory.Apply(connection, userCulture, analyticsDataCulture);
			shiftLength.Apply(connection, userCulture, analyticsDataCulture);
			absence.Apply(connection, userCulture, analyticsDataCulture);
			activity.Apply(connection, userCulture, analyticsDataCulture);

			for (int interval = _shiftStartIntervalId; interval <= _shiftEndIntervalId; interval++)
			{
				var schedule = new FactSchedule(_personId, _shiftStartLocalDateId, _scheduleDateId, 15, 15, 15, interval, _scenarioId, _businessUnitId);
				schedule.Apply(connection, userCulture, analyticsDataCulture);
			}
			
		}
	}
}
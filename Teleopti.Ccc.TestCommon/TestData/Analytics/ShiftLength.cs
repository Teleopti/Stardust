using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class ShiftLength : IAnalyticsDataSetup
	{
		private readonly int _id;
		private readonly int _shiftLength;
		private readonly IDatasourceData _datasource;

		public ShiftLength(
			int id,
			int shiftLength,
			IDatasourceData datasource)
		{
			_id = id;
			_shiftLength = shiftLength;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_shift_length.CreateTable())
			{
				table.AddShiftLength(_id, _shiftLength, _shiftLength/60d, _datasource.RaptorDefaultDatasourceId);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

		public IEnumerable<DataRow> Rows { get; private set; }

	}
}
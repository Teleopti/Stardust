using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class TodayDate : IAnalyticsDataSetup, IDateData
	{
		public IEnumerable<DataRow> Rows { get; set; }

		public int DateId = 0;
		public DateOnly Date;

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_date.CreateTable())
			{
				var date = DateTime.Now.Date;
				Date = new DateOnly(date);
				table.AddDate(DateId, date, analyticsDataCulture);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

	}
}
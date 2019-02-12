using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class DatesFromPeriod : IDateData
	{
		public Dictionary<DateTime, DataRow> DateMap { get; set; }
		public bool CreateDateIdGap { get; set; }
		public IEnumerable<DataRow> Rows { get; set; }
		private readonly DateTime startDate;
		private readonly DateTime endDate;

		public DatesFromPeriod(DateTime startDate, DateTime endDate)
		{
			this.startDate = startDate;
			this.endDate = endDate;
			DateMap = new Dictionary<DateTime, DataRow>();
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var table = dim_date.CreateTable();

			var dates = startDate.DateRange((endDate-startDate).Days);

			var id = 0;
			dates.ForEach(date =>
			{
				var row = table.AddDate(id, date, analyticsDataCulture);
				DateMap[date.Date] = row;
				if (CreateDateIdGap)
				{
					id++;
				}
				id++;
			});

			Bulk.Insert(connection, table);

			Rows = table.AsEnumerable();
		}
	}
}
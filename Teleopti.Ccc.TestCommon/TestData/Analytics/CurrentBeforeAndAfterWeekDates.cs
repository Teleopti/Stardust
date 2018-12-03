using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;


namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class CurrentBeforeAndAfterWeekDates : IDateData
	{
		public DateTime Date { get; set; }
		
		public IEnumerable<DataRow> Rows { get; set; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var table = dim_date.CreateTable();

			var startDate = DateHelper.GetFirstDateInWeek(Date, userCulture).AddDays(-7);
			var dates = startDate.DateRange(21);

			var id = 0;
			dates.ForEach(date =>
			              	{
								table.AddDate(id, date, analyticsDataCulture);
			              		id++;
			              	});

			Bulk.Insert(connection, table);

			Rows = table.AsEnumerable();
		}
	}

}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class TodayDate : IAnalyticsDataSetup, IDateData
	{
		public DataTable Table { get; set; }

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture)
		{
			Table = dim_date.CreateTable();

			var date = DateTime.Now.Date;
			Table.AddDate(0, date, analyticsDataCulture);

			Bulk.Insert(connection, Table);
		}

	}
}
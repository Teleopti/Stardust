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
	public class Activity : IAnalyticsDataSetup, IActivityData
	{
		private readonly IActivity _activity;
		private readonly IDatasourceData _datasource;
		private readonly int _businessUnitId;

		public Activity(IActivity activity,
			IDatasourceData datasource,
			int businessUnitId)
		{
			_activity = activity;
			_datasource = datasource;
			_businessUnitId = businessUnitId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_activity.CreateTable())
			{
				table.AddActivity(ActivityId, _activity.Id.GetValueOrDefault(), _activity.Name, _activity.DisplayColor.ToArgb(), _businessUnitId, _datasource.RaptorDefaultDatasourceId,false);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

		public IEnumerable<DataRow> Rows { get; private set; }
		public int ActivityId { get; set; }

	}
}
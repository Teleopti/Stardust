using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class Activity : IAnalyticsDataSetup, IActivityData
	{
		private readonly int _id;
		private readonly string _name;
		private readonly Guid _code;
		private readonly Color _displayColor;
		private readonly int _datasourceId;
		private readonly int _businessUnitId;

		public Activity(
			int id,
			Guid code,
			string name,
			Color displayColor,
			IDatasourceData datasource,
			int businessUnitId) :this(id, code,name, displayColor, datasource.RaptorDefaultDatasourceId, businessUnitId)
		{
		}
		
		public Activity(
			int id,
			Guid code,
			string name,
			Color displayColor,
			int datasourceId,
			int businessUnitId)
		{
			_id = id;
			_code = code;
			_name = name;
			_displayColor = displayColor;
			_datasourceId = datasourceId;
			_businessUnitId = businessUnitId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var sqlCommand = new SqlCommand("select count(1) from mart.dim_activity where activity_id = @id", connection))
			{
				sqlCommand.Parameters.AddWithValue("@id", _id);
				if (Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
					return;
			}
			using (var table = dim_activity.CreateTable())
			{
				table.AddActivity(_id, _code, _name, _displayColor.ToArgb(), _businessUnitId, _datasourceId, false);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

		public IEnumerable<DataRow> Rows { get; private set; }
		public static Activity NotDefined(IDatasourceData datasource, int businessUnitId)
		{
			return new Activity(-1, Guid.Empty, "Not Defined", Color.Empty, datasource, businessUnitId);
		}
	}
}
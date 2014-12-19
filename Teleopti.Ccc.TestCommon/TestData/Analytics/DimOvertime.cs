using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class DimOvertime : IAnalyticsDataSetup, IActivityData
	{
		private readonly int _id;
		private readonly string _name;
		private readonly Guid _code;
		private readonly IDatasourceData _datasource;
		private readonly int _businessUnitId;

		public DimOvertime(
			int id,
			Guid code,
			string name,
			IDatasourceData datasource,
			int businessUnitId)
		{
			_id = id;
			_code = code;
			_name = name;
			_datasource = datasource;
			_businessUnitId = businessUnitId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_overtime.CreateTable())
			{
				table.AddOvertime(_id, _code, _name, _businessUnitId, _datasource.RaptorDefaultDatasourceId, false);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

		public IEnumerable<DataRow> Rows { get; private set; }
	}
}
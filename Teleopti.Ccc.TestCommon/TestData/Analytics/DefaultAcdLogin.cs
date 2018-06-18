using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class DefaultAcdLogin : IAnalyticsDataSetup
	{
		private readonly int _acdLoginId;
		private readonly int _datasourceId;

		public DefaultAcdLogin(int acdLoginId, int datasourceId)
		{
			_acdLoginId = acdLoginId;
			_datasourceId = datasourceId;
		}
		public IEnumerable<DataRow> Rows { get; set; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_acd_login.CreateTable())
			{
				table.AddDimAcdLogin(_acdLoginId, null, null, "Not Defined", null, null, -1, _datasourceId);
				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}
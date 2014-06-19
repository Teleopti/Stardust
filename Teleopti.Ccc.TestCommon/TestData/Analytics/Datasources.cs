using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class Datasources : IAnalyticsDataSetup
	{
		private readonly int _dataSourceId;
		private readonly string _name;
		private readonly int _logObjectId;
		private readonly string _logObjectName;
		private readonly int _dataSourceDatabaseId;
		private readonly string _dataSourceDatabaseName;
		private readonly string _dataSourceTypeName;
		private readonly int _timeZoneId;
		private readonly bool _inactive;
		private readonly string _sourceId;
		private readonly bool _internal;

		public IEnumerable<DataRow> Rows { get; set; }

		public Datasources(
			int dataSourceId,
			string name,
			int logObjectId,
			string logObjectName,
			int dataSourceDatabaseId,
			string dataSourceDatabaseName,
			string dataSourceTypeName,
			int timeZoneId,
			bool inactive,
			string sourceId,
			bool @internal)
		{
			_dataSourceId = dataSourceId;
			_name = name;
			_logObjectId = logObjectId;
			_logObjectName = logObjectName;
			_dataSourceDatabaseId = dataSourceDatabaseId;
			_dataSourceDatabaseName = dataSourceDatabaseName;
			_dataSourceTypeName = dataSourceTypeName;
			_timeZoneId = timeZoneId;
			_inactive = inactive;
			_sourceId = sourceId;
			_internal = @internal;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = sys_datasource.CreateTable())
			{
				table.AddDataSource(_dataSourceId, _name, _logObjectId, _logObjectName, _dataSourceDatabaseId,
					_dataSourceDatabaseName, _dataSourceTypeName, _timeZoneId, _inactive, _sourceId,
					_internal);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}
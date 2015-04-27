using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class SysConfiguration : IAnalyticsDataSetup
	{
		private readonly IList<KeyValuePair<string, string>> _configuration = new List<KeyValuePair<string, string>>();

		public SysConfiguration(string key, string value)
		{
			AddConfiguration(key, value);
		}

		public void AddConfiguration(string key, string value)
		{
			_configuration.Add(new KeyValuePair<string, string>(key, value));
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = sys_configuration.CreateTable())
			{
				foreach (var keyValuePair in _configuration)
				{
					table.AddConfiguration(keyValuePair.Key, keyValuePair.Value);
				}

				Bulk.Insert(connection, table);
			}
		}
	}
}
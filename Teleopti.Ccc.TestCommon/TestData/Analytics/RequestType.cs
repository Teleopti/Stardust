using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class RequestType : IAnalyticsDataSetup
	{
		private readonly int _id;
		private readonly string _typeName;
		private readonly string _resourceKey;

		public RequestType(int id, string typeName, string resourceKey)
		{
			_id = id;
			_typeName = typeName;
			_resourceKey = resourceKey;
		}


		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_request_type.CreatTable())
			{
				table.AddRequestType(_id, _typeName, _resourceKey);

				Bulk.Insert(connection, table);
			}
		}
	}
}

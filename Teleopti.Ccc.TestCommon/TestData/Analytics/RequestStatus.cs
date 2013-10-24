using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class RequestStatus : IAnalyticsDataSetup
	{
		private readonly int _id;
		private readonly string _statusName;
		private readonly string _resource;

		public RequestStatus(int id, string statusName, string resource)
		{
			_id = id;
			_statusName = statusName;
			_resource = resource;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_request_status.CreateTable())
			{
				table.AddRequestStatus(_id, _statusName, _resource);

				Bulk.Insert(connection, table);
			}
		}
	}
}

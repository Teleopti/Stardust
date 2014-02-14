using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FillBridgeAcdLoginPersonFromData : IAnalyticsDataSetup
	{
		private readonly IPersonData _personData;
		private readonly int _acdId;

		public FillBridgeAcdLoginPersonFromData(IPersonData personData, int acdId)
		{
			_personData = personData;
			_acdId = acdId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = bridge_acd_login_person.CreateTable())
			{
				table.AddAcdLogin(_acdId, _personData.PersonId, 1, 1, 1);

				Bulk.Insert(connection, table);
			}
		}
	}
}
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Model;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class BusinessUnit : IStatisticsDataSetup
	{
		public int BusinessUnitId = 0;
		public DataTable Table;

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			Table = dim_business_unit.CreateTable();

			Table.AddRow(BusinessUnitId, TestData.BusinessUnit.Id.Value, TestData.BusinessUnit.Name, sys_datasource.RaptorDefaultDatasourceId, DateTime.Now, DateTime.Now, DateTime.Now);

			Bulk.Insert(connection, Table);
		}
	}
}
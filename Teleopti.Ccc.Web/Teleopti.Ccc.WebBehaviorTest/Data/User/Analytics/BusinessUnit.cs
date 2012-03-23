using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class BusinessUnit : IAnalyticsDataSetup, IBusinessUnitData
	{
		private readonly IBusinessUnit _businessUnit;

		public int BusinessUnitId { get; private set; }
		public DataTable Table { get; private set; }

		public BusinessUnit(IBusinessUnit businessUnit) {
			_businessUnit = businessUnit;
			BusinessUnitId = 0;
		}

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			Table = dim_business_unit.CreateTable();

			Table.AddRow(BusinessUnitId, _businessUnit.Id.Value, _businessUnit.Name, sys_datasource.RaptorDefaultDatasourceId);

			Bulk.Insert(connection, Table);
		}

	}
}
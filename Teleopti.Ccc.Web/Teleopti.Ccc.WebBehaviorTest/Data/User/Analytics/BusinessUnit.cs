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
		private readonly IDatasourceData _datasource;

		public int BusinessUnitId { get; private set; }
		public DataTable Table { get; private set; }

		public BusinessUnit(IBusinessUnit businessUnit, IDatasourceData datasource) {
			_businessUnit = businessUnit;
			_datasource = datasource;
			BusinessUnitId = 0;
		}

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture)
		{
			Table = dim_business_unit.CreateTable();

			Table.AddBusinessUnit(BusinessUnitId, _businessUnit.Id.Value, _businessUnit.Name, _datasource.RaptorDefaultDatasourceId);

			Bulk.Insert(connection, Table);
		}

	}
}
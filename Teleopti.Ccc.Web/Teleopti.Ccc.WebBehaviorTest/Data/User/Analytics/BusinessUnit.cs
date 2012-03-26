using System;
using System.Collections.Generic;
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

		public int BusinessUnitId { get; set; }
		public IEnumerable<DataRow> Rows { get; set; }

		public BusinessUnit(IBusinessUnit businessUnit, IDatasourceData datasource) {
			_businessUnit = businessUnit;
			_datasource = datasource;
			BusinessUnitId = 0;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_business_unit.CreateTable())
			{
				table.AddBusinessUnit(BusinessUnitId, _businessUnit.Id.Value, _businessUnit.Name, _datasource.RaptorDefaultDatasourceId);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

	}
}
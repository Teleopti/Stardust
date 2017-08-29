using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class BusinessUnit : IAnalyticsDataSetup, IBusinessUnitData
	{
		private readonly IBusinessUnit _businessUnit;
		private readonly IDatasourceData _datasource;
		public static int IdCounter = 0;
		public int BusinessUnitId { get; set; }
		public IEnumerable<DataRow> Rows { get; set; }

		public BusinessUnit(IBusinessUnit businessUnit, IDatasourceData datasource) : this (businessUnit, datasource, 0)
		{
		}

		public BusinessUnit(IBusinessUnit businessUnit, IDatasourceData datasource, int businessUnitId)
		{
			_businessUnit = businessUnit;
			_datasource = datasource;
			BusinessUnitId = businessUnitId;
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
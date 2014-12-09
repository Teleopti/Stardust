using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class Absence : IAnalyticsDataSetup, IAbsenceData
	{
		private readonly IAbsence _absence;
		private readonly IDatasourceData _datasource;
		private readonly int _businessUnitId;

		public Absence(IAbsence absence,
			IDatasourceData datasource,
			int businessUnitId)
		{
			_absence = absence;
			_datasource = datasource;
			_businessUnitId = businessUnitId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_absence.CreateTable())
			{
				table.AddAbsence(AbsenceId, _absence.Id.GetValueOrDefault(), _absence.Name, _absence.DisplayColor.ToArgb(), _businessUnitId, _datasource.RaptorDefaultDatasourceId, false);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

		public IEnumerable<DataRow> Rows { get; private set; }
		public int AbsenceId { get; set; }

	}
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class StageRequest : IAnalyticsDataSetup, IStageRequest
	{
		private readonly IDatasourceData _datasource;
		private readonly Guid _businessUnitCode;

		private readonly Guid _requestCode;
		private readonly Guid _personCode;
		private readonly DateTime _requestDate;
		private readonly int _requestType;
		private readonly int _requestStatus;
		private readonly Guid _absenceCode;


		public IEnumerable<DataRow> Rows { get; set; }

		public StageRequest(Guid requestCode, Guid personCode, DateTime requestDate, int requestType, int requestStatus, 
			Guid absenceCode,  Guid businessUnitCode, IDatasourceData datasource)
		{
			_requestCode = requestCode;
			_personCode = personCode;
			_requestDate = requestDate;
			_requestType = requestType;
			_requestStatus = requestStatus;
			_absenceCode = absenceCode;
			_businessUnitCode = businessUnitCode;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_person.CreateTable())
			{
				table.AddStageRequest(_requestCode, _personCode, _requestDate, _requestType, _requestStatus, _businessUnitCode, _absenceCode, _datasource.RaptorDefaultDatasourceId);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

	}
}
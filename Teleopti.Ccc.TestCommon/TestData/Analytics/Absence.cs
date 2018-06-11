﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class Absence : IAnalyticsDataSetup, IAbsenceData
	{
		private readonly int _id;
		private readonly Guid _code;
		private readonly string _name;
		private readonly Color _displayColor;
		private readonly IDatasourceData _datasource;
		private readonly int _businessUnitId;

		public bool SkipDatasourceUpdateDate { get; set; }

		public Absence(
			int id,
			Guid code,
			string name,
			Color displayColor,
			IDatasourceData datasource,
			int businessUnitId)
		{
			_id = id;
			_code = code;
			_name = name;
			_displayColor = displayColor;
			_datasource = datasource;
			_businessUnitId = businessUnitId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var sqlCommand = new SqlCommand("select count(1) from mart.dim_absence where absence_id = @id", connection))
			{
				sqlCommand.Parameters.AddWithValue("@id", _id);
				if (Convert.ToInt32(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
					return;
			}
			using (var table = dim_absence.CreateTable())
			{
				table.AddAbsence(_id, _code, _name, _displayColor.ToArgb(), _businessUnitId, _datasource.RaptorDefaultDatasourceId, false, SkipDatasourceUpdateDate);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

		public IEnumerable<DataRow> Rows { get; private set; }

		public static Absence NotDefinedAbsence(IDatasourceData datasource, int businessUnitId)
		{
			return new Absence(-1, Guid.Empty, "Not Defined", Color.Empty, datasource, businessUnitId);
		}

	}
}
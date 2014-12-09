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
	public class ShiftCategory : IAnalyticsDataSetup, IShiftCategoryData
	{
		private readonly IShiftCategory _shiftCategory;
		private readonly IDatasourceData _datasource;
		private readonly int _businessUnitId;

		public ShiftCategory(IShiftCategory shiftCategory,
			IDatasourceData datasource,
			int businessUnitId)
		{
			_shiftCategory = shiftCategory;
			_datasource = datasource;
			_businessUnitId = businessUnitId;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_shift_category.CreateTable())
			{
				table.AddShiftCategory(ShiftCategoryId, _shiftCategory.Id.GetValueOrDefault(), _shiftCategory.Description.Name, _shiftCategory.Description.ShortName, _shiftCategory.DisplayColor.ToArgb(), _businessUnitId, _datasource.RaptorDefaultDatasourceId, false);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

		public IEnumerable<DataRow> Rows { get; private set; }
		public int ShiftCategoryId { get; set; }
	}
}
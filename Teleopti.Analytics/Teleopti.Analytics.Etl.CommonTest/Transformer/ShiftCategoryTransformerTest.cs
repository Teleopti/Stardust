using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class ShiftCategoryTransformerTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_shiftCategoryCollection = ShiftCategoryFactory.CreateShiftCategoryCollection();
			var shiftCategoryTransformer = new ShiftCategoryTransformer(_insertDateTime);

			using (var table = new DataTable())
			{
				table.Locale = Thread.CurrentThread.CurrentCulture;
				ShiftCategoryInfrastructure.AddColumnsToDataTable(table);
				shiftCategoryTransformer.Transform(_shiftCategoryCollection, table);

				Assert.AreEqual(4, table.Rows.Count);

				_dataRow0 = table.Rows[0];
				_dataRow1 = table.Rows[1];
				_dataRow2 = table.Rows[2];
				_dataRow3 = table.Rows[3];
			}
		}

		#endregion

		private readonly DateTime _insertDateTime = DateTime.Now;
		private IList<IShiftCategory> _shiftCategoryCollection;
		private DataRow _dataRow0;
		private DataRow _dataRow1;
		private DataRow _dataRow2;
		private DataRow _dataRow3;

		[Test]
		public void VerifyAggregateRoot()
		{
			//Businessunit
			Assert.AreEqual(_shiftCategoryCollection[2].GetOrFillWithBusinessUnit_DONTUSE().Id, _dataRow2["business_unit_code"]);
			Assert.AreEqual(_shiftCategoryCollection[1].GetOrFillWithBusinessUnit_DONTUSE().Description.Name,
								 _dataRow1["business_unit_name"]);
			//UpdatedOn
			Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_shiftCategoryCollection[0]),
								 _dataRow0["datasource_update_date"]);
		}

		[Test]
		public void VerifyShiftCategory()
		{
			Assert.AreEqual(_shiftCategoryCollection[0].Id, _dataRow0["shift_category_code"]);
			Assert.AreEqual(_shiftCategoryCollection[2].Description.Name, _dataRow2["shift_category_name"]);
			Assert.AreEqual(_shiftCategoryCollection[1].Description.ShortName,
								 _dataRow1["shift_category_short_name"]);
			Assert.AreEqual(_shiftCategoryCollection[0].DisplayColor.ToArgb(),
								 Color.FromArgb((int)_dataRow0["display_color"]).ToArgb());
		}

		[Test]
		public void VerifyTheMatrixInternalData()
		{
			Assert.AreEqual(1, _dataRow0["datasource_id"]);
			Assert.AreEqual(_insertDateTime, _dataRow1["insert_date"]);
			Assert.AreEqual(_insertDateTime, _dataRow2["update_date"]);
		}

		[Test]
		public void VerifyIsDeleted()
		{
			Assert.IsFalse((bool)_dataRow0["is_deleted"]);
			Assert.IsTrue((bool)_dataRow3["is_deleted"]);
		}
	}
}
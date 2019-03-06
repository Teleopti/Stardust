using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class OvertimeTransformerTest
	{
		private IList<IMultiplicatorDefinitionSet> _multiplicatorDefinitionSetCollection;
		private readonly DateTime _insertDateTime = DateTime.Now;
		private DataTable _table;

		[SetUp]
		public void Setup()
		{
			_multiplicatorDefinitionSetCollection = OvertimeFactory.CreateMultiplicatorDefinitionSetList();
			var multiplicatorDefinitionSetTransformer = new OvertimeTransformer(_insertDateTime);

			_table = new DataTable();
			_table.Locale = Thread.CurrentThread.CurrentCulture;
			OvertimeInfrastructure.AddColumnsToTable(_table);
			multiplicatorDefinitionSetTransformer.Transform(_multiplicatorDefinitionSetCollection, _table);
		}

		[Test]
		public void ShouldTransformOvertime()
		{
			Assert.AreEqual(2, _table.Rows.Count);
			Assert.AreEqual(_multiplicatorDefinitionSetCollection[0].Id, _table.Rows[0]["overtime_code"]);
			Assert.AreEqual(_multiplicatorDefinitionSetCollection[0].Name, _table.Rows[0]["overtime_name"]);
		}

		[Test]
		public void VerifyAggregateRoot()
		{
			//BusinessUnit
			Assert.AreEqual(_multiplicatorDefinitionSetCollection[0].GetOrFillWithBusinessUnit_DONTUSE().Id, _table.Rows[0]["business_unit_code"]);
			Assert.AreEqual(_multiplicatorDefinitionSetCollection[1].GetOrFillWithBusinessUnit_DONTUSE().Description.Name, _table.Rows[1]["business_unit_name"]);
			//UpdatedOn
			Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_multiplicatorDefinitionSetCollection[0]),
							_table.Rows[0]["datasource_update_date"]);
		}

		[Test]
		public void VerifyTheMatrixInternalData()
		{
			Assert.AreEqual(1, _table.Rows[0]["datasource_id"]);
			Assert.AreEqual(_insertDateTime, _table.Rows[0]["insert_date"]);
			Assert.AreEqual(_insertDateTime, _table.Rows[1]["update_date"]);
		}

		[Test]
		public void VerifyIsDeleted()
		{
			Assert.IsFalse((bool)_table.Rows[0]["is_deleted"]);
			Assert.IsTrue((bool)_table.Rows[1]["is_deleted"]);
		}
	}
}

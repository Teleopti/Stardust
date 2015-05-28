using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class StateGroupTransformerTest
	{
		private IList<IRtaStateGroup> _stateGroupCollection;
		private readonly DateTime _insertDateTime = DateTime.Now;
		private DataTable _table;

		[SetUp]
		public void Setup()
		{
			_stateGroupCollection = StateGroupFactory.CreateStateGroupList();
			var stateGroupTransformer = new StateGroupTransformer(_insertDateTime);
			_table = new DataTable();
			_table.Locale = Thread.CurrentThread.CurrentCulture;
			StateGroupInfrastructure.AddColumnsToDataTable(_table);
			stateGroupTransformer.Transform(_stateGroupCollection, _table);
		}

		[Test]
		public void VerifyStateGroup()
		{
			Assert.AreEqual(3, _table.Rows.Count);
			Assert.AreEqual(_stateGroupCollection[0].Id, _table.Rows[0]["state_group_code"]);
			Assert.AreEqual(_stateGroupCollection[0].Name, _table.Rows[0]["state_group_name"]);
		}

		[Test]
		public void VerifyAggregateRoot()
		{
			//BusinessUnit
			Assert.AreEqual(_stateGroupCollection[0].BusinessUnit.Id, _table.Rows[0]["business_unit_code"]);


			//UpdatedOn
			Assert.AreEqual(RaptorTransformerHelper.GetUpdatedDate(_stateGroupCollection[0]),
									 _table.Rows[0]["datasource_update_date"]);
		}

		[Test]
		public void VerifyTheMatrixInternalData()
		{
			Assert.AreEqual(1, _table.Rows[0]["datasource_id"]);
			Assert.AreEqual(_insertDateTime, _table.Rows[0]["insert_date"]);
			Assert.AreEqual(_insertDateTime, _table.Rows[1]["update_date"]);
		}
	}
}
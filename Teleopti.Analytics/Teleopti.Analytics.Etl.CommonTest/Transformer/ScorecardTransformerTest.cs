using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class ScorecardTransformerTest
	{
		private readonly DateTime _updatedOnDateTime = DateTime.Now;

		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_scorecardList = ScorecardFactory.CreateScorecardCollection(_updatedOnDateTime);
			_target = new ScorecardTransformer();
			_dataTable = _target.Transform(_scorecardList, _insertDateTime);
		}

		#endregion

		private ScorecardTransformer _target;
		private IList<IScorecard> _scorecardList = new List<IScorecard>();
		private readonly DateTime _insertDateTime = DateTime.Now;
		private DataTable _dataTable;

		[Test]
		public void VerifyAggregateRoot()
		{
			//BusinessUnit
			Assert.AreEqual(_scorecardList[0].GetOrFillWithBusinessUnit_DONTUSE().Id, _dataTable.Rows[0]["business_unit_code"]);
			Assert.AreEqual(_scorecardList[1].GetOrFillWithBusinessUnit_DONTUSE().Description.Name, _dataTable.Rows[1]["business_unit_name"]);
			//UpdatedOn
			Assert.AreEqual(_updatedOnDateTime, _dataTable.Rows[0]["datasource_update_date"]);
		}

		[Test]
		public void VerifyScorecard()
		{
			Assert.AreEqual(_scorecardList[0].Id, _dataTable.Rows[0]["scorecard_code"]);
			Assert.AreEqual(_scorecardList[1].Name, _dataTable.Rows[1]["scorecard_name"]);
			Assert.AreEqual(_scorecardList[0].Period.Id, _dataTable.Rows[0]["period"]);
			Assert.AreEqual(_scorecardList[1].Period.Name, _dataTable.Rows[1]["period_name"]);
		}

		[Test]
		public void VerifyTheMatrixInternalData()
		{
			Assert.AreEqual(1, _dataTable.Rows[0]["datasource_id"]);
			Assert.AreEqual(_insertDateTime, _dataTable.Rows[1]["insert_date"]);
			Assert.AreEqual(_insertDateTime, _dataTable.Rows[0]["update_date"]);
			Assert.AreEqual(_updatedOnDateTime, _dataTable.Rows[0]["datasource_update_date"]);
		}
	}
}
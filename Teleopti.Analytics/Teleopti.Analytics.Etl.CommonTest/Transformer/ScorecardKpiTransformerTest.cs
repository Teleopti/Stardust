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
	public class ScorecardKpiTransformerTest
	{
		private ScorecardKpiTransformer _target;
		private IList<IScorecard> _scorecardList = new List<IScorecard>();
		private readonly DateTime _insertDateTime = DateTime.Now;
		private readonly DateTime _updatedOnDateTime = DateTime.Now;
		private DataRow _dataRow0;
		private DataRow _dataRow1;

		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_scorecardList = ScorecardFactory.CreateScorecardCollection(_updatedOnDateTime);
			_target = new ScorecardKpiTransformer(_insertDateTime);
			using (DataTable dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				ScorecardKpiInfrastructure.AddColumnsToDataTable(dataTable);

				_target.Transform(_scorecardList, dataTable);

				Assert.AreEqual(2, dataTable.Rows.Count);

				_dataRow0 = dataTable.Rows[0];
				_dataRow1 = dataTable.Rows[1];
			}
		}

		#endregion

		[Test]
		public void VerifyScorecardKeyPerformanceIndicator()
		{
			Assert.AreEqual(_scorecardList[0].Id, _dataRow0["scorecard_code"]);
			Assert.AreEqual(_scorecardList[0].KeyPerformanceIndicatorCollection[0].Id, _dataRow0["kpi_code"]);
		}

		[Test]
		public void VerifyTheMatrixInternalData()
		{
			Assert.AreEqual(_scorecardList[0].GetOrFillWithBusinessUnit_DONTUSE().Id, _dataRow0["business_unit_code"]);
			Assert.AreEqual(_scorecardList[0].GetOrFillWithBusinessUnit_DONTUSE().Name, _dataRow0["business_unit_name"]);
			Assert.AreEqual(1, _dataRow0["datasource_id"]);
			Assert.AreEqual(_insertDateTime, _dataRow1["insert_date"]);
			Assert.AreEqual(_insertDateTime, _dataRow0["update_date"]);
			Assert.AreEqual(_updatedOnDateTime, _dataRow0["datasource_update_date"]);
		}
	}
}
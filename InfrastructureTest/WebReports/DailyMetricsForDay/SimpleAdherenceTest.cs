using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public class SimpleAdherenceTest
	{
		private TestDataFactory _testDataFactory;
		private AnalyticsDataFactory _analyticsDataFactory;
		private IUnitOfWork _unitOfWork;

		[SetUp]
		public void Setup()
		{
			_unitOfWork = UnitOfWorkFactory.CurrentUnitOfWorkFactory().LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
			_analyticsDataFactory = new AnalyticsDataFactory();
			_testDataFactory = new TestDataFactory(action => action.Invoke(_unitOfWork));
		}

		[TearDown]
		public void Teardown()
		{
			//DataSourceHelper.ClearAnalyticsData();
			_unitOfWork.Dispose();
		}

		[Test]
		public void Dummy()
		{
			var timeZones = new UtcAndCetTimeZones();
			var dates = new TodayDate();
			var intervals = new QuarterOfAnHourInterval();
			var dataSource = new ExistingDatasources(timeZones);

			var agent = new Person(SetupFixtureForAssembly.loggedOnPerson, dataSource, 0, new DateTime(2010, 1, 1),
			           new DateTime(2059, 12, 31), 0, -2, 0, BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value,
			           false);
			var scenario = Scenario.DefaultScenarioFor(1, BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value);

			_analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			_analyticsDataFactory.Setup(timeZones);
			_analyticsDataFactory.Setup(dates);
			_analyticsDataFactory.Setup(intervals);
			_analyticsDataFactory.Setup(dataSource);
			_analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, dataSource));
			_analyticsDataFactory.Setup(agent);
			//acd lgin
			_analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(agent, 1));
			_analyticsDataFactory.Setup(scenario);
			_analyticsDataFactory.Setup(new FactAgentQueue(0, 1, 1, 1, 1, 1, 1, 1, dataSource.RaptorDefaultDatasourceId));

			_analyticsDataFactory.Persist();

			new DailyMetricsForDayQuery().Execute(new DateOnlyPeriod(2000, 1, 1, 2020, 1, 1), 1, 1, SetupFixtureForAssembly.loggedOnPerson, BusinessUnitFactory.BusinessUnitUsedInTest);
		}
	}
}
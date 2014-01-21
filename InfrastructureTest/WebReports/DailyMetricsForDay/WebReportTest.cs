using System;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public abstract class WebReportTest
	{
		private AnalyticsDataFactory _analyticsDataFactory;
		protected ExistingDatasources Datasource;

		[SetUp]
		public void Setup()
		{
			_analyticsDataFactory = new AnalyticsDataFactory();
			insertCommonData();
		}

		private void insertCommonData()
		{
			var timeZones = new UtcAndCetTimeZones();
			var dates = new TodayDate();
			var intervals = new QuarterOfAnHourInterval();
			Datasource = new ExistingDatasources(timeZones);

			var fakeBuId = Guid.NewGuid();

			var agent = new Person(SetupFixtureForAssembly.loggedOnPerson, Datasource, 0, new DateTime(2010, 1, 1),
					   new DateTime(2059, 12, 31), 0, -2, 0, fakeBuId,false);
			var scenario = Scenario.DefaultScenarioFor(1, fakeBuId);

			_analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			_analyticsDataFactory.Setup(timeZones);
			_analyticsDataFactory.Setup(dates);
			_analyticsDataFactory.Setup(intervals);
			_analyticsDataFactory.Setup(Datasource);
			_analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(dates, intervals, timeZones, Datasource));
			_analyticsDataFactory.Setup(agent);
			//acd lgin
			_analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(agent, 1));
			_analyticsDataFactory.Setup(scenario);

			InsertTestSpecificData(_analyticsDataFactory);
			_analyticsDataFactory.Persist();
		}

		protected abstract void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory);

		[TearDown]
		public void Teardown()
		{
			DataSourceHelper.ClearAnalyticsData();
		} 
	}
}
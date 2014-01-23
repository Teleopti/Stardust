using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Person = Teleopti.Ccc.TestCommon.TestData.Analytics.Person;
using Scenario = Teleopti.Ccc.TestCommon.TestData.Analytics.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.WebReports.DailyMetricsForDay
{
	[TestFixture]
	public abstract class WebReportTest
	{
		private AnalyticsDataFactory _analyticsDataFactory;
		protected ExistingDatasources Datasource;
		protected int PersonId;
		protected int AcdLoginId;
		protected int ScenarioId;
		protected TodayDate Today;

		[SetUp]
		public void Setup()
		{
			DataSourceHelper.ClearAnalyticsData();
			_analyticsDataFactory = new AnalyticsDataFactory();
			insertCommonData();
		}

		private void insertCommonData()
		{
			var timeZones = new UtcAndCetTimeZones();
			Today = new TodayDate();
			var intervals = new QuarterOfAnHourInterval();
			Datasource = new ExistingDatasources(timeZones);

			var buId = new CurrentBusinessUnit(new CurrentTeleoptiPrincipal()).Current().Id.Value;
			PersonId = 76;
			AcdLoginId = 123;
			ScenarioId = 12;

			var agent = new Person(SetupFixtureForAssembly.loggedOnPerson, Datasource, PersonId, new DateTime(2010, 1, 1),
					   new DateTime(2059, 12, 31), 0, -2, 0, buId,false);
			var scenario = Scenario.DefaultScenarioFor(ScenarioId, buId);

			_analyticsDataFactory.Setup(new EternityAndNotDefinedDate());
			_analyticsDataFactory.Setup(timeZones);
			_analyticsDataFactory.Setup(Today);
			_analyticsDataFactory.Setup(intervals);
			_analyticsDataFactory.Setup(Datasource);
			_analyticsDataFactory.Setup(new FillBridgeTimeZoneFromData(Today, intervals, timeZones, Datasource));
			_analyticsDataFactory.Setup(agent);
			_analyticsDataFactory.Setup(new FillBridgeAcdLoginPersonFromData(agent, AcdLoginId));
			_analyticsDataFactory.Setup(scenario);

			InsertTestSpecificData(_analyticsDataFactory);
			_analyticsDataFactory.Persist();
		}

		protected abstract void InsertTestSpecificData(AnalyticsDataFactory analyticsDataFactory);

		protected DailyMetricsForDayQuery Target()
		{
			return new DailyMetricsForDayQuery(new CurrentDataSource(new CurrentIdentity()), new CurrentBusinessUnit(new CurrentTeleoptiPrincipal()));
		}
	}
}
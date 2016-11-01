using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class Bug25131
	{
		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.BeginTest();
		}

		[TearDown]
		public void TearDown()
		{
			SetupFixtureForAssembly.EndTest();
		}

		// this is a way of just test an insert via a sp not a "real" integration test
		[Test]
		public void ShouldWork()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			var dates = new CurrentWeekDates();
			var datasource = new ExistingDatasources(timeZones);
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, datasource);
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;

			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());			
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(businessUnit);
			
			analyticsDataFactory.Setup(new Person(person, datasource, 0, new DateTime(2010, 1, 1),
									   new DateTime(2059, 12, 31), 0, -2, 0, TestState.BusinessUnit.Id.GetValueOrDefault(),
									   false, timeZones.UtcTimeZoneId, Guid.NewGuid()));
			analyticsDataFactory.Setup(new Person(person, datasource, 1, new DateTime(2011, 1, 1),
									   new DateTime(2059, 12, 31), 0, -2, 0, TestState.BusinessUnit.Id.GetValueOrDefault(),
										 true, timeZones.UtcTimeZoneId, Guid.NewGuid()));
			analyticsDataFactory.Setup(new StageRequest(Guid.NewGuid(),person.Id.GetValueOrDefault(),DateTime.Now,1,0, 
											Guid.NewGuid(),TestState.BusinessUnit.Id.GetValueOrDefault(),datasource));
			analyticsDataFactory.Persist();

			var raptorRep = new RaptorRepository(InfraTestConfigReader.AnalyticsConnectionString, null, null);
			//so we have a NULL absence
			raptorRep.FillAbsenceDataMart(TestState.BusinessUnit);
		}
	}
}
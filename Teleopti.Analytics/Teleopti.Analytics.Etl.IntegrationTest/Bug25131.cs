using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Generic;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using RequestType = Teleopti.Ccc.TestCommon.TestData.Analytics.RequestType;

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

		[Test]
		public void ShouldWork()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			var dates = new CurrentWeekDates();
			var datasource = new  ExistingDatasources(new UtcAndCetTimeZones());
			var businessUnit = new BusinessUnit(TestState.BusinessUnit, datasource);
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;

			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());			
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(new Person(person, datasource, 0, new DateTime(2010, 1, 1),
									   new DateTime(2059, 12, 31), 0, -2, 0, TestState.BusinessUnit.Id.Value,
									   false));
			analyticsDataFactory.Setup(new Person(person, datasource, 1, new DateTime(2011, 1, 1),
                                       new DateTime(2059, 12, 31), 0, -2, 0, TestState.BusinessUnit.Id.Value,
                                       true));
			analyticsDataFactory.Persist();
				

			
			// sätt upp analytics data här
			Data.Person("Ashley Andeen").Apply(new StockholmTimeZone());
			Data.Apply(new AbsenceConfigurable { Color = "Red", Name = "Absence" });
			

            //Valid from date id måste vi veta här när vi insertar så dom måste insertas först
			
			
			//Act
			// kör etl körning här
			JobParameters parameters = null; //= new JobParameters()
			var steps = new List<JobStepBase>
				{
					new IntradayStageRequestJobStep(parameters),
					new FactRequestJobStep(parameters, true)
				};

			//Assert
			// kolla att prylarna hamnade rätt i databasen här


		}


	}
}
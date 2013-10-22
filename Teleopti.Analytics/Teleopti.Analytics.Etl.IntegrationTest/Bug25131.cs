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
			var businesUnit = new Ccc.Domain.Common.BusinessUnit("bu");
			businesUnit.SetId(Guid.NewGuid());
			var businessUnit = new BusinessUnit(businesUnit, datasource);
			var requestType = new RequestType(1, "Absence Request", "absence");
			var requestStatus = new RequestStatus(1, "Pending", "pending");

			analyticsDataFactory.Setup(new EternityAndNotDefinedDate());			
			analyticsDataFactory.Setup(dates);
			analyticsDataFactory.Setup(businessUnit);
			analyticsDataFactory.Setup(requestType);
			analyticsDataFactory.Setup(requestStatus);
			analyticsDataFactory.Persist();

			Data.Person("Ashley Andeen").Apply(new StockholmTimeZone());
			Data.Apply(new AbsenceConfigurable { Color = "Red", Name = "Absence" });
			Data.Person("Ashley Andeen").Apply(new AbsenceRequestConfigurable
				{
					Absence = "Absence",
					StartTime = DateTime.Parse("2013-10-22 00:00"),
					EndTime = DateTime.Parse("2013-10-23 00:00")
				});
			// sätt upp analytics data här??

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
using System;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups;

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
			//Arrange
			Data.Person("Ashley Andeen").Apply(new StockholmTimeZone());
			//Data.Person("Ashley Andeen").Apply(new +
			//    {
			//        StartDate = DateTime.Parse("2013-10-21")
			//    });
			//Data.Person("Ashley Andeen").Apply(new PersonPeriodConfigurable
			//    {
			//        StartDate = DateTime.Parse("2013-10-22")
			//    });

			//Data.Apply(new AbsenceConfigurable{Color = "Red", Name = "Absence"});
			//Data.Person("Ashley Andeen").Apply(new AbsenceRequestConfigurable
			//    {
			//        Absence = "Absence",
			//        StartTime = DateTime.Parse("2013-10-22 00:00"),
			//        EndTime = DateTime.Parse("2013-10-23 00:00")
			//    });

			// sätt upp analytics data här
			var analFakta = new AnalyticsDataFactory();
			var person = TestState.TestDataFactory.Person("Ashley Andeen").Person;

			var datasource = new  ExistingDatasources(new UtcAndCetTimeZones());
			analFakta.Setup(new BusinessUnit(TestState.BusinessUnit, datasource));

            //Valid from date id måste vi veta här när vi insertar så dom måste insertas först
			analFakta.Setup(new Person(person, datasource, 0, new DateTime(2010, 1, 1),
									   new DateTime(2059, 12, 31), 0, -2, 0, TestState.BusinessUnit.Id.Value,
									   false));
            analFakta.Setup(new Person(person, datasource, 1, new DateTime(2011, 1, 1),
                                       new DateTime(2059, 12, 31), 0, -2, 0, TestState.BusinessUnit.Id.Value,
                                       true));
			analFakta.Persist();

			//Act
			// kör etl körning här

			//Assert
			// kolla att prylarna hamnade rätt i databasen här


		}

	   
	}
}
using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Generic;
using Teleopti.Ccc.TestCommon.TestData.Setups;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class Bug25131
	{
		[SetUp]
		public void Setup()
		{
			SetupFixtureForAssembly.SetupDatabase();
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

	
			Data.Apply(new AbsenceConfigurable{Color = "Red", Name = "Absence"});
			Data.Person("Ashley Andeen").Apply(new AbsenceRequestConfigurable
				{
					Absence = "Absence",
					StartTime = DateTime.Parse("2013-10-22 00:00"),
					EndTime = DateTime.Parse("2013-10-23 00:00")
				});
			// sätt upp analytics data här??

			//Act
			// kör etl körning här

			//Assert
			// kolla att prylarna hamnade rätt i databasen här


		}

	   
	}
}
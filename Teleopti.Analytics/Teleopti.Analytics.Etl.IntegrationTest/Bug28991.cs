using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class Bug28991
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
		public void ShouldWorkForBrazilTimeZone()
		{
			DateTime testDate = new DateTime(2013, 06, 15);

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);

			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			var personName = "Ola H";
			BasicShiftSetup.AddPerson(out person, personName, "", testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Person("Ola H").Apply(new BrasilianTimeZone());
			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

			BasicShiftSetup.AddThreeShifts(personName, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity, testDate);

			var dateList = new JobMultipleDate(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC",15,"","False",CultureInfo.CurrentCulture)
				{
					Helper =
						new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null, null)
				};

			//run nightly
			StepRunner.RunNightly(jobParameters);
			
			// now it should have data on all three dates 96 interval
			var factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

			//now set a leaving date
			//person.TerminatePerson(new DateOnly(testDate.AddDays(0)), new PersonAccountUpdaterDummy());

			var personFactory = Data.Person(personName);
			var leavingDateForUser = new LeavingDateForUser
				{
					LeavingDate = new DateOnly(testDate.AddDays(0))
				};
			personFactory.Apply(leavingDateForUser);

			//run nightly
			jobParameters = null;
			jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null, null)
			};

			StepRunner.RunNightly(jobParameters);

			// still it should have data on all three dates 96 interval
			factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(64));
		}

		[Test]
		public void ShouldWorkForAustralianTimeZone()
		{
			DateTime testDate = new DateTime(2013, 06, 15);

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>(), testDate);

			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			var personName = "Ola H";
			
			BasicShiftSetup.AddPerson(out person, personName, "", testDate);

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };
			Data.Person("Ola H").Apply(new AustralianTimeZone());
			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

			BasicShiftSetup.AddThreeShifts(personName, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity, testDate);

			var dateList = new JobMultipleDate(TimeZoneInfoFactory.StockholmTimeZoneInfo());
			dateList.Add(testDate.AddDays(-3), testDate.AddDays(3), JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null, null)
			};

			//run nightly
			StepRunner.RunNightly(jobParameters);

			// now it should have data on all three dates 96 interval
			var factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(96));

			//now set a leaving date
			//person.TerminatePerson(new DateOnly(testDate.AddDays(0)), new PersonAccountUpdaterDummy());

			var personFactory = Data.Person(personName);
			var leavingDateForUser = new LeavingDateForUser
			{
				LeavingDate = new DateOnly(testDate.AddDays(0))
			};
			personFactory.Apply(leavingDateForUser);

			//run nightly
			jobParameters = null;
			jobParameters = new JobParameters(dateList, 1, "UTC", 15, "", "False", CultureInfo.CurrentCulture)
			{
				Helper =
					new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""), null, null, null)
			};

			StepRunner.RunNightly(jobParameters);

			// still it should have data on all three dates 96 interval
			factSchedules = SqlCommands.RowsInFactSchedule();
			Assert.That(factSchedules, Is.EqualTo(64));
		}
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Teleopti.Analytics.Etl.IntegrationTest.Models;
using Teleopti.Analytics.Etl.IntegrationTest.TestData;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.IntegrationTest
{
	[TestFixture]
	public class Bug26500
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
		public void ShouldWorkForStockholm()
		{
			// run this to get a date and time in mart.LastUpdatedPerStep
			var etlUpdateDate = new EtlReadModelSetup { BusinessUnit = TestState.BusinessUnit, StepName = "Schedules" };
			Data.Apply(etlUpdateDate);

			AnalyticsRunner.RunAnalyticsBaseData(new List<IAnalyticsDataSetup>());

			var cat = new ShiftCategoryConfigurable { Name = "Kattegat", Color = "Green" };
			var activityPhone = new ActivityConfigurable { Name = "Phone", Color = "LightGreen", InReadyTime = true };
			var activityLunch = new ActivityConfigurable { Name = "Lunch", Color = "Red" };

			Data.Apply(cat);
			Data.Apply(activityPhone);
			Data.Apply(activityLunch);

			IPerson person;
			BasicShiftSetup.SetupBasicForShifts();
			BasicShiftSetup.AddPerson(out person, "Ola H", "");
			BasicShiftSetup.AddThreeShifts("Ola H", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			IPerson person2;
			BasicShiftSetup.AddPerson(out person2, "David J", "");
			BasicShiftSetup.AddThreeShifts("David J", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);
			
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			dateList.Add(DateTime.Today.AddDays(-3),DateTime.Today.AddDays(3),JobCategoryType.Schedule);
			var jobParameters = new JobParameters(dateList, 1, "UTC",15,"","False",CultureInfo.CurrentCulture)
				{
					Helper =
						new JobHelper(new RaptorRepository(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix, ""),null,null)
				};

			//transfer site, team contract etc from app to analytics
			StepRunner.RunNightly(jobParameters);

			// now it should have data on all three dates on both persons 192 interval
			var db = new AnalyticsContext(ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
			var factSchedules = from s in db.fact_schedule select s;
			Assert.That(factSchedules.Count(), Is.EqualTo(192));

			//edit three shifts for Ola
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", DateTime.Today.AddDays(-1), person);
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", DateTime.Today.AddDays(0), person);
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "Ola H", DateTime.Today.AddDays(1), person);
			BasicShiftSetup.AddThreeShifts("Ola H", cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			//edit one shift for David
			RemovePersonSchedule.RemoveAssignmentAndReadmodel(BasicShiftSetup.Scenario.Scenario, "David J", DateTime.Today.AddDays(0), person2);
			BasicShiftSetup.AddShift("David J", DateTime.Today.AddDays(0), 10, 8, cat.ShiftCategory, activityLunch.Activity, activityPhone.Activity);

			StepRunner.RunIntraday(jobParameters);

			factSchedules = from s in db.fact_schedule select s;

			Assert.That(factSchedules.Count(), Is.EqualTo(192));
		}
	}
}
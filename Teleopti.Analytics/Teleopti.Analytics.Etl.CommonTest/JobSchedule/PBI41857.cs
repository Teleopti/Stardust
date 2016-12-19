using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.CommonTest.Transformer.Job;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using WorkloadFactory = Teleopti.Ccc.TestCommon.FakeData.WorkloadFactory;

namespace Teleopti.Analytics.Etl.CommonTest.JobSchedule
{
	[DomainTest]
	public class PBI41857
	{
		[Test, Ignore("Claes - This is green but it shouldnt...")]
		public void ShouldShovel()
		{
			var date = new DateOnly(2016, 12, 20);
			var dateAsUtc = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			dateList.Add(date.Date, date.Date, JobCategoryType.Schedule);
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var primarySkill = new Skill("primary", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			primarySkill.SetCascadingIndex(1);
			var secondarySkill = new Skill("secondary", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(primarySkill);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(secondarySkill);
			var skillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>
			{
				[primarySkill] = new[] {primarySkill.CreateSkillDayWithDemand(scenario, date, 0.5)},
				[secondarySkill] = new[] {secondarySkill.CreateSkillDayWithDemand(scenario, date, 2)}
			};
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(primarySkill, secondarySkill);
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 24));
			var agent2 = new Person().InTimeZone(TimeZoneInfo.Utc).KnowsSkill(primarySkill, secondarySkill);
			var ass2 = new PersonAssignment(agent2, scenario, date);
			ass2.AddActivity(activity, new TimePeriod(0, 24));
			var scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, new DateTimePeriod(dateAsUtc, dateAsUtc.AddDays(1)), ass, ass2);
			var raptorRep = new RaptorRepositoryForTest();
			raptorRep.SetLoadSkillWithSkillDays(new [] {primarySkill, secondarySkill});
			raptorRep.SetLoadScenario(scenario);
			raptorRep.SetLoadSkillDays(skillDays);
			raptorRep.SetLoadSchedule(scheduleDictionary);
			var jobHelper = new JobHelperForTest(raptorRep, null);
			var containerHolder = new JobParametersFactory.FakeContainerHolder();
			var jobParameters = new JobParameters(dateList, 0, "UTC", 15, "", "", null, containerHolder, false) { Helper = jobHelper};
			var target = new StageScheduleForecastSkillJobStep(jobParameters) {ClearDataTablesAfterRun = false};

			target.Run(new List<IJobStep>(), null, null, false);

			var result = target.BulkInsertDataTable1;
			const double expectedScheduledOnPrimary = 0.5;
			const double expectedScheduledOnSecondary = 1.5;
			var rowsOnDate = result.Rows.Cast<DataRow>().Where(x => (DateTime)x["date"] == dateAsUtc);
			var primaryRows = rowsOnDate.Where(x => (Guid) x["skill_code"] == primarySkill.Id.Value);
			var secondaryRows = rowsOnDate.Where(x => (Guid) x["skill_code"] == secondarySkill.Id.Value);
			primaryRows.All(x => Math.Abs((double)x["scheduled_resources"]) - expectedScheduledOnPrimary < 0.01)
				.Should().Be.True();
			secondaryRows.All(x => Math.Abs((double)x["scheduled_resources"]) - expectedScheduledOnSecondary < 0.01)
				.Should().Be.True();
		}
	}
}
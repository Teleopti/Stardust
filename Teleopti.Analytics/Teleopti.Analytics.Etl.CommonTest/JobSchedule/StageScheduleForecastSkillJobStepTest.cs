using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.Job;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Analytics.Etl.CommonTest.JobSchedule
{
	[DomainTest]
	public class StageScheduleForecastSkillJobStepTest : IExtendSystem
	{
		public IComponentContext ComponentContext;

		[Test]
		public void ShouldCalculateResourceBasedOnPrimarySkillAndBpo()
		{
			//Dates
			var date = new DateOnly(2016, 12, 20);
			var dateAsUtc = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			dateList.Add(date.Date, date.Date, JobCategoryType.Schedule);

			// Scenario, Activity and skills
			var scenario = new Scenario("_") {DefaultScenario = true};
			var activity = new Activity("_").WithId();
			var primarySkill = new Skill("primary", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			WorkloadFactory.CreateWorkloadWithFullOpenHours(primarySkill);

			var skillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>
			{
				[primarySkill] = new[] { primarySkill.CreateSkillDayWithDemand(scenario, date, 10) },
			};

			// Agent, Assignment and Schedule
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill).WithId();
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 24));

			var period = new DateTimePeriod(dateAsUtc, dateAsUtc.AddDays(1));
			var scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, period, data:ass);

			var externalStaff = new List<ExternalStaff>();
			var intervals = period.Intervals(TimeSpan.FromMinutes(15));

			foreach (var interval in intervals)
				externalStaff.Add(new ExternalStaff(1, new[] { primarySkill }, interval));

			//RaptorRep
			var raptorRep = new RaptorRepositoryForTest();
			raptorRep.SetLoadSkillWithSkillDays(new[] { primarySkill });
			raptorRep.SetLoadScenario(scenario);
			raptorRep.SetLoadSkillDays(skillDays);
			raptorRep.SetLoadSchedule(scheduleDictionary);
			raptorRep.SetLoadExternalStaffData(externalStaff);

			var jobHelper = new JobHelperForTest(raptorRep, null);
			var containerHolder = new IocContainerHolder(ComponentContext);
			var jobParameters = new JobParameters(dateList, 0, "UTC", 15, "", "", null, containerHolder, false) { Helper = jobHelper };
			var target = new StageScheduleForecastSkillJobStepWithBpo(jobParameters) { ClearDataTablesAfterRun = false };
			var res = target.Run(new List<IJobStep>(), null, null, false);

			// assertions
			res.JobStepException.Should().Be.Null();
			var result = target.BulkInsertDataTable1;
			const double expectedScheduledOnPrimary = 2;
			var rowsOnDate = result.Rows.Cast<DataRow>().Where(x => (DateTime)x["date"] == dateAsUtc);
			var primaryRows = rowsOnDate.Where(x => (Guid)x["skill_code"] == primarySkill.Id.Value);

			foreach (var primaryRow in primaryRows)
			{
				primaryRow["scheduled_resources"].Should().Be.EqualTo(expectedScheduledOnPrimary);
			}
		}

		[Test]
		public void ShouldShovelWithoutDefaultScenarioAndNotUsingBpo()
		{
			var date = new DateOnly(2016, 12, 20);
			var dateAsUtc = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			dateList.Add(date.Date, date.Date, JobCategoryType.Schedule);
			var scenario = new Scenario("_") { DefaultScenario = false };
			var activity = new Activity("_").WithId();
			var primarySkill = new Skill("primary", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			primarySkill.SetCascadingIndex(1);
			var secondarySkill = new Skill("secondary", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(primarySkill);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(secondarySkill);
			var skillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>
			{
				[primarySkill] = new[] { primarySkill.CreateSkillDayWithDemand(scenario, date, 0.5) },
				[secondarySkill] = new[] { secondarySkill.CreateSkillDayWithDemand(scenario, date, 2) }
			};
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, secondarySkill).WithId();
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 24));

			var period = new DateTimePeriod(dateAsUtc, dateAsUtc.AddDays(1));
			var scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, period, data:ass);
		
			var intervals = period.Intervals(TimeSpan.FromMinutes(15));
			var externalStaff = new List<ExternalStaff>();
			foreach (var interval in intervals)
				externalStaff.Add(new ExternalStaff(1, new[] { primarySkill, secondarySkill }, interval));

			var raptorRep = new RaptorRepositoryForTest();
			raptorRep.SetLoadSkillWithSkillDays(new[] { primarySkill, secondarySkill });
			raptorRep.SetLoadScenario(scenario);
			raptorRep.SetLoadSkillDays(skillDays);
			raptorRep.SetLoadSchedule(scheduleDictionary);
			raptorRep.SetLoadExternalStaffData(externalStaff);
			var jobHelper = new JobHelperForTest(raptorRep, null);
			var containerHolder = new IocContainerHolder(ComponentContext);
			var jobParameters = new JobParameters(dateList, 0, "UTC", 15, "", "", null, containerHolder, false) { Helper = jobHelper };
			var target = new StageScheduleForecastSkillJobStepWithBpo(jobParameters) { ClearDataTablesAfterRun = false };

			var res = target.Run(new List<IJobStep>(), null, null, false);
			res.JobStepException.Should().Be.Null();

			var result = target.BulkInsertDataTable1;
			const double expectedScheduledOnPrimary = 0.5;
			const double expectedScheduledOnSecondary = 0.5;
			var rowsOnDate = result.Rows.Cast<DataRow>().Where(x => (DateTime)x["date"] == dateAsUtc);
			var primaryRows = rowsOnDate.Where(x => (Guid)x["skill_code"] == primarySkill.Id.Value);
			var secondaryRows = rowsOnDate.Where(x => (Guid)x["skill_code"] == secondarySkill.Id.Value);

			foreach (var primaryRow in primaryRows)
			{
				primaryRow["scheduled_resources"].Should().Be.EqualTo(expectedScheduledOnPrimary);
			}

			foreach (var secondaryRow in secondaryRows)
			{
				secondaryRow["scheduled_resources"].Should().Be.EqualTo(expectedScheduledOnSecondary);
			}
		}

		[Test]
		public void ShouldShovelWithDefaultScenario()
		{
			var date = new DateOnly(2016, 12, 20);
			var dateAsUtc = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateList = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			dateList.Add(date.Date, date.Date, JobCategoryType.Schedule);
			var scenario = new Scenario("_") { DefaultScenario = true };
			var activity = new Activity("_").WithId();
			var primarySkill = new Skill("primary", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			primarySkill.SetCascadingIndex(1);
			var secondarySkill = new Skill("secondary", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)) { Activity = activity, TimeZone = TimeZoneInfo.Utc }.WithId();
			secondarySkill.SetCascadingIndex(2);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(primarySkill);
			WorkloadFactory.CreateWorkloadWithFullOpenHours(secondarySkill);
			var skillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>
			{
				[primarySkill] = new[] { primarySkill.CreateSkillDayWithDemand(scenario, date, 0.5) },
				[secondarySkill] = new[] { secondarySkill.CreateSkillDayWithDemand(scenario, date, 2) }
			};
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(primarySkill, secondarySkill).WithId();
			var ass = new PersonAssignment(agent, scenario, date);
			ass.AddActivity(activity, new TimePeriod(0, 24));

			var period = new DateTimePeriod(dateAsUtc, dateAsUtc.AddDays(1));
			var scheduleDictionary = ScheduleDictionaryForTest.WithScheduleDataForManyPeople(scenario, period,data: ass);

			var intervals = period.Intervals(TimeSpan.FromMinutes(15));
			var externalStaff = new List<ExternalStaff>();
			foreach (var interval in intervals)
				externalStaff.Add(new ExternalStaff(1, new[] { primarySkill, secondarySkill }, interval));

			var raptorRep = new RaptorRepositoryForTest();
			raptorRep.SetLoadSkillWithSkillDays(new[] { primarySkill, secondarySkill });
			raptorRep.SetLoadScenario(scenario);
			raptorRep.SetLoadSkillDays(skillDays);
			raptorRep.SetLoadSchedule(scheduleDictionary);
			raptorRep.SetLoadExternalStaffData(externalStaff);
			var jobHelper = new JobHelperForTest(raptorRep, null);
			var containerHolder = new IocContainerHolder(ComponentContext);
			var jobParameters = new JobParameters(dateList, 0, "UTC", 15, "", "", null, containerHolder, false) { Helper = jobHelper };
			var target = new StageScheduleForecastSkillJobStepWithBpo(jobParameters) { ClearDataTablesAfterRun = false };

			var res = target.Run(new List<IJobStep>(), null, null, false);
			res.JobStepException.Should().Be.Null();

			var result = target.BulkInsertDataTable1;
			const double expectedScheduledOnPrimary = 0.5;
			const double expectedScheduledOnSecondary = 1.5;
			var rowsOnDate = result.Rows.Cast<DataRow>().Where(x => (DateTime)x["date"] == dateAsUtc).ToList();
			var primaryRows = rowsOnDate.Where(x => (Guid)x["skill_code"] == primarySkill.Id.Value);
			var secondaryRows = rowsOnDate.Where(x => (Guid)x["skill_code"] == secondarySkill.Id.Value);

			foreach (var primaryRow in primaryRows)
			{
				primaryRow["scheduled_resources"].Should().Be.EqualTo(expectedScheduledOnPrimary);
			}

			foreach (var secondaryRow in secondaryRows)
			{
				secondaryRow["scheduled_resources"].Should().Be.EqualTo(expectedScheduledOnSecondary);
			}
		}

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new EtlModule(configuration));
		}
	}
}

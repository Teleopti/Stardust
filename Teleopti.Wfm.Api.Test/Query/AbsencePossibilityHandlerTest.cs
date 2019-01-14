using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Wfm.Api.Test.Query
{
	[ApiTest]
	[LoggedOnAppDomain]
	public class AbsencePossibilityHandlerTest : IExtendSystem
	{
		private const int intervalLengthInMinute = 15;

		public IApiHttpClient Client;
		public IAbsenceRepository AbsenceRepository;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeWorkloadRepository WorkloadRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;
		public MutableNow Now;
		public FullPermission Permissions;

		[TestCase(10, 5)]
		[TestCase(10, 10)]
		[TestCase(10, 20)]
		public async System.Threading.Tasks.Task ShouldGetAbsencePossibility(double forecastStaffPerInterval, double scheduledStaffPerInterval)
		{
			var today = new DateTime(2018, 8, 2, 0, 0, 0, DateTimeKind.Utc);
			var tomorrow = today.AddDays(1);

			var dateOnlyToday = new DateOnly(today);

			Now.Is(today.AddHours(13));

			Client.Authorize();

			var absence = AbsenceFactory.CreateAbsence("Absence").WithId();
			absence.Requestable = true;
			AbsenceRepository.Add(absence);

			var scenario = ScenarioRepository.Has("Default").WithId();
			var activity = ActivityRepository.Has("Test Activity").WithId();

			var openHours = new TimePeriod(0, 24);
			var skill = createSkill(intervalLengthInMinute, "Phone", openHours, activity);
			SkillRepository.Has(skill);

			setupScheduleData(skill, today, forecastStaffPerInterval, scheduledStaffPerInterval);
			setupScheduleData(skill, tomorrow, forecastStaffPerInterval, scheduledStaffPerInterval);

			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(today.Year, 1, 1), new[] {skill}).WithId();
			var workflowControlSet = createWorkflowControlSet(today, absence);
			person.WorkflowControlSet = workflowControlSet;

			PersonRepository.Has(person);

			for (var i = -1; i <= 1; i++)
			{
				var personAssignment = new PersonAssignment(person, scenario, dateOnlyToday.AddDays(i));
				personAssignment.AddActivity(activity, new TimePeriod(8, 17));
				PersonAssignmentRepository.Add(personAssignment);
			}

			var queryDto = new
			{
				PersonId = person.Id,
				StartDate = today,
				EndDate = tomorrow
			};

			var result = await Client.PostAsync("/query/AbsencePossibility/AbsencePossibilityByPersonId",
				new StringContent(JsonConvert.SerializeObject(queryDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());

			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(true);
			resultDto["Result"].Count().Should().Be.EqualTo((17 - 8) * 4 * 2);
			Console.WriteLine(resultDto["Result"].Count(x => x["Possibility"].Value<int>() == 1));
			resultDto["Result"].All(x => x["Possibility"].Value<int>() == 1).Should().Be
				.EqualTo(forecastStaffPerInterval < scheduledStaffPerInterval);
		}

		[Test]
		public async System.Threading.Tasks.Task ShouldDenyGetAbsencePossibilityWhenPersonNotFound()
		{
			var today = new DateTime(2018, 8, 2, 0, 0, 0, DateTimeKind.Utc);
			var tomorrow = today.AddDays(1);

			Now.Is(today.AddHours(13));
			Client.Authorize();

			var personId = Guid.NewGuid();
			var queryDto = new
			{
				PersonId = personId,
				StartDate = today,
				EndDate = tomorrow
			};

			var result = await Client.PostAsync("/query/AbsencePossibility/AbsencePossibilityByPersonId",
				new StringContent(JsonConvert.SerializeObject(queryDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());

			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(false);
			resultDto["Message"].Value<string>().Should().Be.EqualTo($"Person with Id {personId} could not be found");
		}

		[Test]
		public async System.Threading.Tasks.Task ShouldDenyGetAbsencePossibilityWhenNotPermitted()
		{
			var today = new DateTime(2018, 8, 2, 0, 0, 0, DateTimeKind.Utc);
			var tomorrow = today.AddDays(1);

			var dateOnlyToday = new DateOnly(today);

			Now.Is(today.AddHours(13));

			Permissions.AddToBlackList(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb);
			Client.Authorize();

			var absence = AbsenceFactory.CreateAbsence("Absence").WithId();
			absence.Requestable = true;
			AbsenceRepository.Add(absence);

			var scenario = ScenarioRepository.Has("Default").WithId();
			var activity = ActivityRepository.Has("Test Activity").WithId();

			var openHours = new TimePeriod(0, 24);
			var skill = createSkill(intervalLengthInMinute, "Phone", openHours, activity);
			SkillRepository.Has(skill);
			
			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(today.Year, 1, 1), new[] {skill})
				.WithId();
			var workflowControlSet = createWorkflowControlSet(today, absence);
			person.WorkflowControlSet = workflowControlSet;

			PersonRepository.Has(person);

			for (var i = -1; i <= 1; i++)
			{
				var personAssignment = new PersonAssignment(person, scenario, dateOnlyToday.AddDays(i));
				personAssignment.AddActivity(activity, new TimePeriod(8, 17));
				PersonAssignmentRepository.Add(personAssignment);
			}

			var queryDto = new
			{
				PersonId = person.Id,
				StartDate = today,
				EndDate = tomorrow
			};

			var result = await Client.PostAsync("/query/AbsencePossibility/AbsencePossibilityByPersonId",
				new StringContent(JsonConvert.SerializeObject(queryDto), Encoding.UTF8, "application/json"));
			var resultDto = JObject.Parse(await result.EnsureSuccessStatusCode().Content.ReadAsStringAsync());

			resultDto["Successful"].Value<bool>().Should().Be.EqualTo(false);
			resultDto["Message"].Value<string>().Should().Be.EqualTo($"Person with Id {person.Id} is not allowed to request absence in {today:yyyy-MM-dd}");
		}

		private static WorkflowControlSet createWorkflowControlSet(DateTime today, IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet
			{
				SchedulePublishedToDate = today.AddYears(1)
			}.WithId();

			workflowControlSet.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				BetweenDays = new MinMax<int>(0, 3),
				OpenForRequestsPeriod = new DateOnlyPeriod(today.Year, 1, 1, today.Year + 2, 12, 31),
				AbsenceRequestProcess = new ApproveAbsenceRequestWithValidators(),
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdWithShrinkageValidator()
			});
			return workflowControlSet;
		}

		private void setupScheduleData(ISkill skill, DateTime date, double forecastStaffingPerInterval = 1,
			double scheduledStaffingPerInterval = 1)
		{
			var staffingPeriods = new List<StaffingPeriodData>();
			var startTime = TimeSpan.FromMinutes(0);
			while (startTime < TimeSpan.FromDays(1))
			{
				var endTime = startTime.Add(TimeSpan.FromMinutes(intervalLengthInMinute));
				staffingPeriods.Add(new StaffingPeriodData
				{
					Period = new DateTimePeriod(date.Date.Add(startTime), date.Date.Add(endTime)),
					ForecastedStaffing = forecastStaffingPerInterval,
					ScheduledStaffing = scheduledStaffingPerInterval
				});

				startTime = endTime;
			}

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill, new DateOnly(date), staffingPeriods,
				TimeZoneInfo.Utc);
		}

		private ISkill createSkill(int intervalLength, string skillName, TimePeriod openHours, IActivity activity)
		{
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			var skill = new Skill(skillName, skillName, Color.Empty, intervalLength, skillType)
			{
				TimeZone = TimeZoneInfo.Utc,
				Activity = activity
			}.WithId();
			
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<SkillIntradayStaffingFactory>();
		}
	}
}

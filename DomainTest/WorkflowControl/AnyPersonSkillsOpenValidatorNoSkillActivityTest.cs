using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[DomainTest]
	[Toggle(Toggles.WFM_AbsenceRequest_ImproveThroughput_79139)]
	public class AnyPersonSkillsOpenValidatorNoSkillActivityOptimizationOnTest
	{
		public IAnyPersonSkillsOpenValidator Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeSkillRepository SkillRepository;
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario Scenario;
		public FakeScenarioRepository ScenarioRepository;

		private IPerson _person;
		private Absence _absence;
		private ISkill _skill;

		[SetUp]
		public void Setup()
		{
			_skill = SkillFactory.CreateSkillWithWorkloadAndSources();

			var date = new DateOnly(2016, 4, 1);
			_person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_person.AddSkill(_skill, date);
			_absence = new Absence();
		}

		[Test]
		public void ShouldNotDenyRequestSpanningOnlyNonSkilledActivitiesWithinSchedule()
		{
			ScenarioRepository.Add(new Scenario("Current Scenario") { DefaultScenario = true });
			PersonAssignmentRepository.Has(_person, Scenario.Current(), new Activity("as"), new ShiftCategory("category"), new DateOnly(2017, 10, 21),
				new TimePeriod(7, 8));
			setupOpenHours(_skill, true);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 7, 2017, 10, 21, 8))).WithId();
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(new DateOnly(2017, 10, 21), new DateOnly(2017, 10, 21)), Scenario.Current());

			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection,
				scheduleDictionary[_person]);

			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldDenyRequestSpanningSkilledActivitiesOutsideOpenHoursWithinSchedule()
		{
			ScenarioRepository.Add(new Scenario("Current Scenario") { DefaultScenario = true });
			PersonAssignmentRepository.Has(_person, Scenario.Current(), new Activity("as") { RequiresSkill = true }, new ShiftCategory("category"), new DateOnly(2017, 10, 21),
				new TimePeriod(7, 8));
			setupOpenHours(_skill, true);

			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 7, 2017, 10, 21, 8))).WithId();
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(new DateOnly(2017, 10, 21), new DateOnly(2017, 10, 21)), Scenario.Current());

			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection,
				scheduleDictionary[_person]);

			validatedRequest.IsValid.Should().Be.False();
		}

		private void setupOpenHours(ISkill skill, bool openWeekend = false)
		{
			var timePeriod = new TimePeriod(TimeSpan.FromTicks(288000000000), TimeSpan.FromTicks(648000000000));
			if (openWeekend)
			{
				skill.IsOpen(new Dictionary<DayOfWeek, TimePeriod>
				{
					{ DayOfWeek.Friday, timePeriod},
					{ DayOfWeek.Monday, timePeriod},
					{ DayOfWeek.Tuesday, timePeriod},
					{ DayOfWeek.Wednesday, timePeriod},
					{ DayOfWeek.Thursday, timePeriod},
					{ DayOfWeek.Sunday, timePeriod},
					{ DayOfWeek.Saturday, timePeriod}
				});
			}
			else
			{
				skill.IsOpen(new Dictionary<DayOfWeek, TimePeriod>
				{
					{ DayOfWeek.Friday, timePeriod},
					{ DayOfWeek.Monday, timePeriod},
					{ DayOfWeek.Tuesday, timePeriod},
					{ DayOfWeek.Wednesday, timePeriod},
					{ DayOfWeek.Thursday, timePeriod}
				});
			}
			SkillRepository.Add(skill);
		}
	}

	[DomainTest]
	[ToggleOff(Toggles.WFM_AbsenceRequest_ImproveThroughput_79139)]
	public class AnyPersonSkillsOpenValidatorNoSkillActivityOptimizationOffTest
	{
		public readonly IAnyPersonSkillsOpenValidator Target;
		private IPerson _person;
		private Absence _absence;
		private ISkill _skill;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IScheduleStorage ScheduleStorage;
		public ICurrentScenario Scenario;
		public FakeScenarioRepository ScenarioRepository;

		[SetUp]
		public void Setup()
		{
			_skill = SkillFactory.CreateSkill("Phone");
			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 5).ToArray();
			WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(_skill, timePeriods);
			var date = new DateOnly(2016, 4, 1);
			_person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			_person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			_person.AddSkill(_skill, date);
			_absence = new Absence();
		}

		[Test]
		public void ShouldNotDenyRequestSpanningOnlyNonSkilledActivitiesWithinSchedule()
		{
			ScenarioRepository.Add(new Scenario("Current Scenario") { DefaultScenario = true });
			PersonAssignmentRepository.Has(_person, Scenario.Current(), new Activity("as"), new ShiftCategory("category"), new DateOnly(2017, 10, 21),
				new TimePeriod(7, 8));
			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 7).ToArray();
			WorkloadFactory.CreateWorkloadWithOpenHours(_skill, timePeriods);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 7, 2017, 10, 21, 8))).WithId();
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(new DateOnly(2017, 10, 21), new DateOnly(2017, 10, 21)), Scenario.Current());

			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection,
				scheduleDictionary[_person]);

			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldDenyRequestSpanningSkilledActivitiesOutsideOpenHoursWithinSchedule()
		{
			ScenarioRepository.Add(new Scenario("Current Scenario") { DefaultScenario = true });
			PersonAssignmentRepository.Has(_person, Scenario.Current(), new Activity("as") { RequiresSkill = true }, new ShiftCategory("category"), new DateOnly(2017, 10, 21),
				new TimePeriod(7, 8));
			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 7).ToArray();
			WorkloadFactory.CreateWorkloadWithOpenHours(_skill, timePeriods);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 7, 2017, 10, 21, 8))).WithId();
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(_person, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(new DateOnly(2017, 10, 21), new DateOnly(2017, 10, 21)), Scenario.Current());

			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection,
				scheduleDictionary[_person]);

			validatedRequest.IsValid.Should().Be.False();
		}
	}



}
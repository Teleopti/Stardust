using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	[DomainTest]
	[Toggle(Toggles.WFM_AbsenceRequest_ImproveThroughput_79139)]
	public class AnyPersonSkillsOpenValidatorTestOpt
	{
		public IAnyPersonSkillsOpenValidator Target;
		public FakeSkillRepository SkillRepository;

		private IPerson _person;
		private Absence _absence;
		private ISkill _skill;
		private IScheduleRange _scheduleRange;

		[SetUp]
		public void Setup2()
		{
			_skill = SkillFactory.CreateSkill("Phone").WithId();
			
			var date = new DateOnly(2016, 4, 1);
			_person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			_person.AddSkill(_skill, date);
			_absence = new Absence();

			var scenario = new Scenario();
			var parameters = new ScheduleParameters(scenario, _person, new DateTimePeriod(2017, 10, 19, 0, 2017, 10, 24, 23));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, DateTime.Now);
			_scheduleRange = new ScheduleRange(scheduleDictionary, parameters, new ByPassPersistableScheduleDataPermissionChecker(), CurrentAuthorization.Make());
		}

		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsClosed()
		{
			setupOpenHours(_skill);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 22, 8, 2017, 10, 22, 18))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.DenyOption.Should().Be.EqualTo(PersonRequestDenyOption.AllPersonSkillsClosed);
			validatedRequest.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldApproveRequestIfAnyPersonSkillIsOpen()
		{
			setupOpenHours(_skill);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 8, 2017, 10, 23, 18))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldApproveRequestIfAnyPersonSkillIsOpenOnSomeHours()
		{
			setupOpenHours(_skill);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 6, 2017, 10, 23, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsIsClosedInEvening()
		{
			setupOpenHours(_skill);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 19, 2017, 10, 23, 20))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsAreInactive()
		{
			setupOpenHours(_skill);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 6, 2017, 10, 23, 9))).WithId();
			_person.DeactivateSkill(_skill, _person.PersonPeriodCollection.First());
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldHandleMultiDayRequest()
		{
			setupOpenHours(_skill);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 23, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldHandleSkillsInDifferentTimezones()
		{
			_skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			setupOpenHours(_skill);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 23, 13))).WithId();
			var request2 = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 23, 12))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			var validatedRequest2 = Target.Validate(request2.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
			validatedRequest2.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldHandleMultiplePersonSkills()
		{
			var skill2 = SkillFactory.CreateSkill("Mail");
			setupOpenHours(_skill);
			setupOpenHours(skill2, true);
			_person.AddSkill(skill2, new DateOnly(2016, 4, 1));
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 22, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldHandleMultipleWorkloads()
		{
			setupOpenHours(_skill, true);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 22, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldHandleMultisiteSkill()
		{
			var skill = SkillFactory.CreateMultisiteSkill("multi", SkillTypeFactory.CreateSkillType(), 15).WithId();
			var child = SkillFactory.CreateChildSkill("child", skill).WithId();
			setupOpenHours(skill, true);
			var date = new DateOnly(2016, 4, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			person.AddSkill(child, date);
			var request = new PersonRequest(person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 22, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldHandleSkillWithNoOpenHours()
		{
			var skill = SkillFactory.CreateSkill("Phone").WithId();
			var date = new DateOnly(2016, 4, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			person.AddSkill(skill, date);
			person.AddSkill(_skill, date);
			setupOpenHours(_skill, true);
			var request = new PersonRequest(person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 22, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldHandleNoSkillWithOpenHours()
		{
			var date = new DateOnly(2016, 4, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			person.AddSkill(_skill, date);
			var request = new PersonRequest(person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 22, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
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
	public class AnyPersonSkillsOpenValidatorTest
	{
		public readonly IAnyPersonSkillsOpenValidator Target;
		private IPerson _person;
		private Absence _absence;
		private ISkill _skill;
		private IScheduleRange _scheduleRange;

		[SetUp]
		public void Setup()
		{
			_skill = SkillFactory.CreateSkill("Phone");
			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 5).ToArray();
			WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(_skill, timePeriods);
			var date = new DateOnly(2016, 4, 1);
			_person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			_person.AddSkill(_skill, date);
			_absence = new Absence();

			var scenario = new Scenario();
			var parameters = new ScheduleParameters(scenario, _person, new DateTimePeriod(2017, 10, 19, 0, 2017, 10, 24, 23));
			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, DateTime.Now);
			_scheduleRange = new ScheduleRange(scheduleDictionary, parameters, new ByPassPersistableScheduleDataPermissionChecker(), CurrentAuthorization.Make());
		}

		[Test]
		public void ShouldApproveRequestIfAnyPersonSkillIsOpen()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 8, 2017, 10, 23, 18))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldApproveRequestIfAnyPersonSkillIsOpenOnSomeHours()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 6, 2017, 10, 23, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsIsClosedInEvening()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 19, 2017, 10, 23, 20))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsAreInactive()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 6, 2017, 10, 23, 9))).WithId();
			_person.DeactivateSkill(_skill, _person.PersonPeriodCollection.First());
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldHandleMultiDayRequest()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 23, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldHandleSkillsInDifferentTimezones()
		{
			_skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 23, 13))).WithId();
			var request2 = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 23, 12))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			var validatedRequest2 = Target.Validate(request2.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
			validatedRequest2.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldHandleMultiplePersonSkills()
		{
			var skill2 = SkillFactory.CreateSkill("Mail");
			_person.AddSkill(skill2, new DateOnly(2016, 4, 1));
			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 7).ToArray();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill2, timePeriods);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 22, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldHandleMultipleWorkloads()
		{
			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 7).ToArray();
			WorkloadFactory.CreateWorkloadWithOpenHours(_skill, timePeriods);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 22, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}

		[Test]
		public void ShouldHandleMultisiteSkill()
		{
			var skill = SkillFactory.CreateMultisiteSkill("multi", SkillTypeFactory.CreateSkillType(), 15);
			var child = SkillFactory.CreateChildSkill("child", skill);

			var date = new DateOnly(2016, 4, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			person.AddSkill(child, date);

			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 7).ToArray();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, timePeriods);
			var request = new PersonRequest(person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 21, 8, 2017, 10, 22, 9))).WithId();
			var validatedRequest = Target.Validate(request.Request as IAbsenceRequest, person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}
	}
}
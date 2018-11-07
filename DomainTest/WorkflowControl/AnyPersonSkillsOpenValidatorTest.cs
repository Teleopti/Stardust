using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	public class AnyPersonSkillsOpenValidatorTest
	{
		private readonly AnyPersonSkillsOpenValidator _target = new AnyPersonSkillsOpenValidator(new FakeSkillRepository());
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
		public void ShouldDenyRequestIfAllPersonSkillsClosed()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,22,8,2017,10,22,18))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.DenyOption.Should().Be.EqualTo(PersonRequestDenyOption.AllPersonSkillsClosed);
			validatedRequest.IsValid.Should().Be.False();
		}
		
		[Test]
		public void ShouldApproveRequestIfAnyPersonSkillIsOpen()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,23,8,2017,10,23,18))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}
		
		[Test]
		public void ShouldApproveRequestIfAnyPersonSkillIsOpenOnSomeHours()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,23,6,2017,10,23,9))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}
		
		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsIsClosedInEvening()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,23,19,2017,10,23,20))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsAreInactive()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017, 10, 23, 6, 2017, 10, 23, 9))).WithId();
			_person.DeactivateSkill(_skill,_person.PersonPeriodCollection.First());
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.False();
		}

		[Test]
		public void ShouldHandleMultiDayRequest()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,21,8,2017,10,23,9))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}
		
		[Test]
		public void ShouldHandleSkillsInDifferentTimezones()
		{
			_skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,21,8,2017,10,23,13))).WithId();
			var request2 = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,21,8,2017,10,23,12))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			var validatedRequest2 = _target.Validate(request2.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
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
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,21,8,2017,10,22,9))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}		
		
		[Test]
		public void ShouldHandleMultipleWorkloads()
		{
			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 7).ToArray();
			WorkloadFactory.CreateWorkloadWithOpenHours(_skill, timePeriods);
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,21,8,2017,10,22,9))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
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
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, person.PersonPeriodCollection.First().PersonSkillCollection, _scheduleRange);
			validatedRequest.IsValid.Should().Be.True();
		}
	}
}
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl
{
	public class AllPersonSkillsOpenValidatorTest
	{
		private readonly AnyPersonSkillsOpenValidator _target = new AnyPersonSkillsOpenValidator();
		private IPerson _person;
		private Absence _absence;

		[SetUp]
		public void Setup()
		{
			var skill = SkillFactory.CreateSkill("Phone");
			var timePeriods = Enumerable.Repeat(new TimePeriod(8, 18), 5).ToArray();
			WorkloadFactory.CreateWorkloadClosedOnWeekendsWithOpenHours(skill, timePeriods);
			//skill.WorkloadCollection.First().TemplateWeekCollection[0].OpenHourList[0].
			var date = new DateOnly(2016, 4, 1);
			_person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(date);
			_person.AddSkill(skill, date);
			_absence = new Absence();
		}
		
		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsClosed()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,22,8,2017,10,22,18))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection);
			validatedRequest.DenyOption.Should().Be.EqualTo(PersonRequestDenyOption.AllPersonSkillsClosed);
			validatedRequest.IsValid.Should().Be.False();
		}
		
		[Test]
		public void ShouldApproveRequestIfAnyPersonSkillIsOpen()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,23,8,2017,10,23,18))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection);
			validatedRequest.IsValid.Should().Be.True();
		}
		
		[Test]
		public void ShouldApproveRequestIfAnyPersonSkillIsOpenOnSomeHours()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,23,6,2017,10,23,9))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection);
			validatedRequest.IsValid.Should().Be.True();
		}
		
		[Test]
		public void ShouldDenyRequestIfAllPersonSkillsIsClosedInEvening()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,23,19,2017,10,23,20))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection);
			validatedRequest.IsValid.Should().Be.False();
		}
		
		[Test]
		public void ShouldHandleMultiDayRequest()
		{
			var request = new PersonRequest(_person, new AbsenceRequest(_absence, new DateTimePeriod(2017,10,21,8,2017,10,23,9))).WithId();
			var validatedRequest = _target.Validate(request.Request as IAbsenceRequest, _person.PersonPeriodCollection.First().PersonSkillCollection);
			validatedRequest.IsValid.Should().Be.True();
		}
	}
}
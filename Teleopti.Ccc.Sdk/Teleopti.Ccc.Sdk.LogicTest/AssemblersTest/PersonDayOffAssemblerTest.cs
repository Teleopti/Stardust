using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonDayOffAssemblerTest
    {
        private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

	    [Test]
	    public void VerifyDoToDto()
	    {
		    var person = PersonFactory.CreatePerson().WithId();
		    person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
		    var scenario = ScenarioFactory.CreateScenarioAggregate();
		    var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
				    new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
		    var date = new DateOnly(1900, 1, 1);
		    var anchorTime = new TimeSpan(23);
		    var flexibility = new TimeSpan(124);
		    var length = new TimeSpan(1244);

		    var dayOff = DayOffFactory.CreateDayOff(new Description("test"));
		    dayOff.Anchor = anchorTime;
		    dayOff.SetTargetAndFlexibility(length, flexibility);
		    var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, date, dayOff).WithId();

			var target = new PersonDayOffAssembler(personAssembler, new DateTimePeriodAssembler());
			target.Person = person;
			target.DefaultScenario = scenario;
			var dayOffDto = target.DomainEntityToDto(personDayOff);

		    Assert.IsNotNull(dayOffDto);
		    Assert.AreEqual(personDayOff.Person.Id.GetValueOrDefault(), dayOffDto.Person.Id.GetValueOrDefault());
		    Assert.AreEqual(personDayOff.Id.GetValueOrDefault(), dayOffDto.Id.GetValueOrDefault());
		    Assert.AreEqual(personDayOff.Period.StartDateTime, dayOffDto.Period.UtcStartTime);
	    }

	    [Test]
	    public void VerifyDoToDtoWithNoDayOffInAssignment()
		{
			var person = PersonFactory.CreatePerson().WithId();
			person.PermissionInformation.SetDefaultTimeZone(_timeZoneInfo);
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
			var personDayOff = PersonAssignmentFactory.CreatePersonAssignment(person).WithId();

			var target = new PersonDayOffAssembler(personAssembler, new DateTimePeriodAssembler());
			target.Person = person;
			target.DefaultScenario = scenario;
			var dayOffDto = target.DomainEntityToDto(personDayOff);
		    Assert.IsNull(dayOffDto);
	    }
    }
}
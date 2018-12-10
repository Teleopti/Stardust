using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class StudentAvailabilityAssemblerTest
    {
	    [Test]
	    public void CanCreateDtoOfDomainObject()
	    {
		    var person = new Person().WithName(new Name("ett namn bara", "")).WithId();
		    var dateOnly = new DateOnly(2009, 2, 2);
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
		    var personRepository = new FakePersonRepositoryLegacy();
		    personRepository.Add(person);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy());
		    var target = new StudentAvailabilityAssembler();
		    target.PersonAssembler = personAssembler;

		    var restriction1 = new StudentAvailabilityRestriction();
		    restriction1.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(8, 0, 0), null);
		    restriction1.EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(15, 0, 0));

		    var restriction2 = new StudentAvailabilityRestriction();
		    restriction2.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(10, 0, 0), null);
		    restriction2.EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(17, 0, 0));
		    restriction2.WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(3, 0, 0), new TimeSpan(6, 0, 0));
		    var restrictions = new List<IStudentAvailabilityRestriction> { restriction1, restriction2 };

		    var day = new StudentAvailabilityDay(person, dateOnly, restrictions);
		    var dto = target.DomainEntityToDto(day);

		    Assert.AreEqual(day.RestrictionCollection.Count, dto.StudentAvailabilityRestrictions.Count);
		    Assert.AreEqual(day.NotAvailable, dto.NotAvailable);
		    Assert.AreEqual(day.RestrictionCollection[0].StartTimeLimitation.StartTime,
			    dto.StudentAvailabilityRestrictions[0].StartTimeLimitation.MinTime);
		    Assert.AreEqual(day.RestrictionCollection[0].EndTimeLimitation.EndTime,
			    dto.StudentAvailabilityRestrictions[0].EndTimeLimitation.MaxTime);

		    Assert.AreEqual(day.RestrictionCollection[1].StartTimeLimitation.StartTime,
			    dto.StudentAvailabilityRestrictions[1].StartTimeLimitation.MinTime);
		    Assert.AreEqual(day.RestrictionCollection[1].EndTimeLimitation.EndTime,
			    dto.StudentAvailabilityRestrictions[1].EndTimeLimitation.MaxTime);
		    Assert.AreEqual(day.RestrictionCollection[1].WorkTimeLimitation.StartTime,
			    dto.StudentAvailabilityRestrictions[1].WorkTimeLimitation.MinTime);
		    Assert.AreEqual(day.RestrictionCollection[1].WorkTimeLimitation.EndTime,
			    dto.StudentAvailabilityRestrictions[1].WorkTimeLimitation.MaxTime);
		    Assert.AreEqual(person.Name.FirstName, dto.Person.Name);
	    }

	    [Test]
	    public void CanCreateDomainObjectOfDtoNotAvailable()
	    {
		    var person = new Person().WithName(new Name("ett namn bara", "")).WithId();
		    var dateOnly = new DateOnly(2009, 2, 2);
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
		    var personRepository = new FakePersonRepositoryLegacy();
		    personRepository.Add(person);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy());
		    var target = new StudentAvailabilityAssembler();
		    target.PersonAssembler = personAssembler;

		    var dto = new StudentAvailabilityDayDto();
		    dto.Person = new PersonDto {Id = person.Id, Name = person.Name.ToString()};
		    dto.RestrictionDate = new DateOnlyDto {DateTime = dateOnly.Date};
		    dto.NotAvailable = true;

		    var day = target.DtoToDomainEntity(dto);
		    Assert.AreEqual(dto.StudentAvailabilityRestrictions.Count, day.RestrictionCollection.Count);
		    Assert.IsTrue(day.NotAvailable);
		    Assert.AreEqual(person, day.Person);
	    }

	    [Test]
	    public void CanCreateDomainObjectOfDto()
	    {
		    var person = new Person().WithName(new Name("ett namn bara", "")).WithId();
		    var dateOnly = new DateOnly(2009, 2, 2);
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
		    var personRepository = new FakePersonRepositoryLegacy();
		    personRepository.Add(person);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
				    new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy());
		    var target = new StudentAvailabilityAssembler();
		    target.PersonAssembler = personAssembler;

		    var dto = new StudentAvailabilityDayDto();
		    dto.Person = new PersonDto {Id = person.Id, Name = person.Name.ToString()};
		    dto.RestrictionDate = new DateOnlyDto {DateTime = dateOnly.Date};
		    var restrictionDto = new StudentAvailabilityRestrictionDto();

		    var startLimitation = new StartTimeLimitation(new TimeSpan(5, 0, 0), null);
		    var endLimitation = new EndTimeLimitation(null, new TimeSpan(14, 0, 0));
		    restrictionDto.StartTimeLimitation = new TimeLimitationDto {MinTime = startLimitation.StartTime};
		    restrictionDto.EndTimeLimitation = new TimeLimitationDto {MaxTime = endLimitation.EndTime};
		    restrictionDto.WorkTimeLimitation = new TimeLimitationDto {MinTime = new TimeSpan(4, 0, 0), MaxTime = new TimeSpan(9, 0, 0)};
		    dto.StudentAvailabilityRestrictions.Add(restrictionDto);

		    var day = target.DtoToDomainEntity(dto);
		    Assert.AreEqual(dto.StudentAvailabilityRestrictions.Count, day.RestrictionCollection.Count);
		    Assert.AreEqual(day.RestrictionCollection[0].StartTimeLimitation.StartTime,
			    dto.StudentAvailabilityRestrictions[0].StartTimeLimitation.MinTime);
		    Assert.AreEqual(day.RestrictionCollection[0].EndTimeLimitation.EndTime,
			    dto.StudentAvailabilityRestrictions[0].EndTimeLimitation.MaxTime);
		    Assert.AreEqual(day.RestrictionCollection[0].WorkTimeLimitation.StartTime,
			    dto.StudentAvailabilityRestrictions[0].WorkTimeLimitation.MinTime);
		    Assert.AreEqual(day.RestrictionCollection[0].WorkTimeLimitation.EndTime,
			    dto.StudentAvailabilityRestrictions[0].WorkTimeLimitation.MaxTime);
	    }
    }
}

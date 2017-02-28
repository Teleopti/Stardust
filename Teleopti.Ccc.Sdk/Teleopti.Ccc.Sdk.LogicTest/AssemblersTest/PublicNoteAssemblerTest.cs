﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PublicNoteAssemblerTest
    {
	    [Test]
	    public void ShouldTransformDomainObjectToDto()
	    {
		    var personDomain = new Person().WithName(new Name("Bosse", "Bäver")).WithId();
		    var date = new DateOnly(2011, 1, 12);

		    var publicNoteDomain =
			    new PublicNote(personDomain, date, new Scenario("Default scenario"), "Work harder!").WithId();
		    
		    var repository = new FakePublicNoteRepository();

		    var personRepository = new FakePersonRepositoryLegacy();
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
		    var activityRepository = new FakeActivityRepository();
		    var activityAssembler = new ActivityAssembler(activityRepository);
		    var dayOffTemplateRepository = new FakeDayOffTemplateRepository();
		    var dayOffAssembler = new DayOffAssembler(dayOffTemplateRepository);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(shiftCategoryAssembler,
				    dayOffAssembler, activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy(),
			    new TenantPeopleLoader(new FakeTenantLogonDataManager()));

		    var target = new PublicNoteAssembler(repository, personAssembler);

		    var publicNoteDto = target.DomainEntityToDto(publicNoteDomain);
		    Assert.AreEqual(publicNoteDomain.Id, publicNoteDto.Id);
		    Assert.AreEqual(publicNoteDomain.Person.Id, publicNoteDto.Person.Id);
		    Assert.AreEqual(publicNoteDomain.Person.Name.ToString(), publicNoteDto.Person.Name);
		    Assert.AreEqual(publicNoteDomain.GetScheduleNote(new NormalizeText()), publicNoteDto.ScheduleNote);
		    Assert.AreEqual(publicNoteDomain.NoteDate.Date, publicNoteDto.Date.DateTime);
	    }

	    [Test]
	    public void ShouldTransformDtoObjectToDomain()
	    {
		    var personDomain = new Person().WithName(new Name("Bosse", "Bäver")).WithId();
		    var date = new DateOnly(2011, 1, 12);

		    var publicNoteDomain =
			    new PublicNote(personDomain, date, new Scenario("Default scenario"), "Work harder!").WithId();
			var repository = new FakePublicNoteRepository();
			repository.Add(publicNoteDomain);
			var publicNoteDto = new PublicNoteDto {Id = publicNoteDomain.Id};
			
		    var personRepository = new FakePersonRepositoryLegacy();
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
		    var activityRepository = new FakeActivityRepository();
		    var activityAssembler = new ActivityAssembler(activityRepository);
		    var dayOffTemplateRepository = new FakeDayOffTemplateRepository();
		    var dayOffAssembler = new DayOffAssembler(dayOffTemplateRepository);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(shiftCategoryAssembler,
				    dayOffAssembler, activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy(),
			    new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			
		    var target = new PublicNoteAssembler(repository, personAssembler);
		    var domainEntity = target.DtoToDomainEntity(publicNoteDto);
		    Assert.IsNotNull(domainEntity);
	    }
    }
}
using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[TestFixture]
	public class PersonAssemblerTest
	{
		private IPerson CreatePerson(bool createWorkflowControlSet)
		{
			var person = PersonFactory.CreatePerson("testuser", "123").WithId();
			person.PermissionInformation.SetCulture(new CultureInfo(1053));
			person.PermissionInformation.SetUICulture(new CultureInfo(1025));
			person.WithName(new Name("aaa", "bbb"));
			person.Email = "email";
			person.SetEmploymentNumber("email");
			person.Note = "A very good agent";
			person.TerminatePerson(new DateOnly(2011, 8, 20), new PersonAccountUpdaterDummy());
			((IDeleteTag)person).SetDeleted();

			if (createWorkflowControlSet)
			{
				var workflowControlSet = new WorkflowControlSet("Controlset");
				person.WorkflowControlSet = workflowControlSet;
			}

			var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1));
			period.SetParent(person);
			period.PersonContract.ContractSchedule.SetId(Guid.NewGuid());
			person.AddPersonPeriod(period);
			person.ChangeTeam(TeamFactory.CreateTeamWithId("team1"), period);

			return person;
		}

		[Test]
		public void VerifyDomainEntityToDto()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var workflowControlSetAssembler =
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository()));
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			
			var target = new PersonAssembler(personRepository, workflowControlSetAssembler, personAccountUpdater);

			var person = CreatePerson(true);
			
			var personDto = target.DomainEntityToDto(person);

			Assert.AreEqual(person.Id, personDto.Id);
			Assert.AreEqual(person.Name.ToString(), personDto.Name);
			Assert.AreEqual(person.Name.FirstName, personDto.FirstName);
			Assert.AreEqual(person.Name.LastName, personDto.LastName);
			Assert.AreEqual(person.Email, personDto.Email);
			Assert.AreEqual(person.EmploymentNumber, personDto.EmploymentNumber);
			Assert.AreEqual(person.PermissionInformation.CultureLCID(), personDto.CultureLanguageId);
			Assert.AreEqual(person.PermissionInformation.UICultureLCID(), personDto.UICultureLanguageId);
			Assert.AreEqual(person.PermissionInformation.DefaultTimeZone().Id, personDto.TimeZoneId);
			Assert.AreEqual(person.PersonPeriodCollection.Count, personDto.PersonPeriodCollection.Count);
			Assert.AreEqual(person.WorkflowControlSet.Id, personDto.WorkflowControlSet.Id);
			Assert.AreEqual(person.Note, personDto.Note);
			Assert.AreEqual(person.TerminalDate.Value.Date, personDto.TerminationDate.DateTime);
			Assert.AreEqual(person.FirstDayOfWeek, personDto.FirstDayOfWeek);
			
			Assert.AreEqual(((IDeleteTag)person).IsDeleted, personDto.IsDeleted);
			Assert.AreEqual(person.PersonPeriodCollection.FirstOrDefault().Team.Id.Value, personDto.PersonPeriodCollection.FirstOrDefault().Team.Id.Value);
			Assert.AreEqual(person.PersonPeriodCollection.FirstOrDefault().PersonContract.ContractSchedule.Id.Value, personDto.PersonPeriodCollection.FirstOrDefault().PersonContract.ContractScheduleId);
		}

		[Test]
		public void ShouldMapDomainEntityWithoutWorkflowControlSetToDto()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var workflowControlSetAssembler =
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository()));
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			
			var target = new PersonAssembler(personRepository, workflowControlSetAssembler, personAccountUpdater);

			var person = CreatePerson(false);
			var personDto = target.DomainEntityToDto(person);

			Assert.That(personDto.WorkflowControlSet, Is.Null);
		}

		[Test]
		public void VerifyCanTransformToDomainObject()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var workflowControlSetAssembler =
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository()));
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			
			var target = new PersonAssembler(personRepository, workflowControlSetAssembler, personAccountUpdater);
			var person = CreatePerson(true);
			personRepository.Add(person);
			
			var personDto = target.DomainEntityToDto(person);
			personDto.CultureLanguageId = 1053;
			personDto.UICultureLanguageId = 1025;
			personDto.FirstName = "aaa";
			personDto.LastName = "bbb";
			personDto.IsDeleted = true;

			var personDo = target.DtoToDomainEntity(personDto);

			Assert.AreEqual(personDto.Id, personDo.Id);
			Assert.AreEqual(personDto.Name, personDo.Name.ToString());
			Assert.AreEqual(personDto.FirstName, personDo.Name.FirstName);
			Assert.AreEqual(personDto.LastName, personDo.Name.LastName);
			Assert.AreEqual(personDto.Email, personDo.Email);
			Assert.AreEqual(personDto.EmploymentNumber, personDo.EmploymentNumber);
			Assert.AreEqual(personDo.PermissionInformation.UICultureLCID(), 1025);
			Assert.AreEqual(personDo.PermissionInformation.CultureLCID(), 1053);

			Assert.AreEqual(personDto.Note, personDo.Note);
			Assert.AreEqual(personDto.TimeZoneId, personDo.PermissionInformation.DefaultTimeZone().DisplayName);
			Assert.AreEqual(personDto.TerminationDate.DateTime, personDo.TerminalDate.Value.Date);
			Assert.AreEqual(personDto.Note, personDo.Note);
			Assert.AreEqual(personDto.IsDeleted, ((Person)personDo).IsDeleted);
		}

		[Test]
		public void VerifyCanTransformToDomainObjectFromNewPersonDto()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var workflowControlSetAssembler =
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository()));
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			
			var target = new PersonAssembler(personRepository, workflowControlSetAssembler, personAccountUpdater);
			var personDto = new PersonDto
			{
				ApplicationLogOnName = "kallek",
				ApplicationLogOnPassword = "mammamu",
				Email = "kalle.kula@teleopti.com",
				EmploymentNumber = "abc123",
				FirstName = "Kalle",
				LastName = "Kula",
				TimeZoneId = "W. Europe Standard Time",
				WorkflowControlSet = null,
				CultureLanguageId = 1053,
				UICultureLanguageId = 1025,
				Note = "Moahaha",
				IsDeleted = true
			};

			var personDo = target.DtoToDomainEntity(personDto);

			Assert.AreEqual(personDto.Id, personDo.Id);
			Assert.AreEqual(personDto.FirstName, personDo.Name.FirstName);
			Assert.AreEqual(personDto.LastName, personDo.Name.LastName);
			Assert.AreEqual(personDto.Email, personDo.Email);
			Assert.AreEqual(personDto.EmploymentNumber, personDo.EmploymentNumber);
			Assert.AreEqual(personDto.UICultureLanguageId, personDo.PermissionInformation.UICultureLCID());
			Assert.AreEqual(personDto.CultureLanguageId, personDo.PermissionInformation.CultureLCID());
			Assert.AreEqual(personDto.CultureLanguageId, personDo.PermissionInformation.CultureLCID());
			Assert.AreEqual(personDto.UICultureLanguageId, personDo.PermissionInformation.UICultureLCID());
			Assert.AreEqual(personDto.IsDeleted, ((IDeleteTag)personDo).IsDeleted);
		}

		[Test]
		public void VerifyCannotAddPersonWithoutName()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var workflowControlSetAssembler =
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository()));
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			
			var target = new PersonAssembler(personRepository, workflowControlSetAssembler, personAccountUpdater);

			string timeZone = "W. Europe Standard Time";
			int culture = 1053;

			var personDto = new PersonDto { TimeZoneId = timeZone, CultureLanguageId = culture };

			Assert.Throws<ArgumentException>(() => target.DtoToDomainEntity(personDto));
		}

		[Test]
		public void VerifyCannotUpdatePersonWithoutName()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var workflowControlSetAssembler =
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository()));
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			var target = new PersonAssembler(personRepository, workflowControlSetAssembler, personAccountUpdater);
			var personId = Guid.NewGuid();
			personRepository.Add(PersonFactory.CreatePerson().WithId(personId));

			string timeZone = "W. Europe Standard Time";
			int culture = 1053;

			var personDto = new PersonDto { Id = personId };
			personDto.TimeZoneId = timeZone;
			personDto.CultureLanguageId = culture;

			target.EnableSaveOrUpdate = true;
			Assert.Throws<ArgumentException>(() => target.DtoToDomainEntity(personDto));
		}

		[Test]
		public void VerifyCannotAddPersonWithoutTimeZone()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var workflowControlSetAssembler =
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository()));
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			
			var target = new PersonAssembler(personRepository, workflowControlSetAssembler, personAccountUpdater);
			string firstName = "Kalle";
			string lastName = "Kula";
			int culture = 1053;

			var personDto = new PersonDto { FirstName = firstName, LastName = lastName, CultureLanguageId = culture };

			Assert.Throws<ArgumentException>(() => target.DtoToDomainEntity(personDto));
		}

		[Test]
		public void ShouldUseDummyAccountUpdaterWhenNewPerson()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			var workflowControlSetAssembler =
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository()));
			var personAccountUpdater = new PersonAccountUpdaterDummy();
			
			var target = new PersonAssembler(personRepository, workflowControlSetAssembler, personAccountUpdater);
			var personDto = new PersonDto
			{
				FirstName = "Personliga",
				LastName = "Person",
				CultureLanguageId = 1053,
				TimeZoneId = "W. Europe Standard Time",
				TerminationDate = new DateOnlyDto(2014, 4, 11)
			};

			target.DtoToDomainEntity(personDto);
			personAccountUpdater.CallCount.Should().Be.EqualTo(0);
		}
	}
}

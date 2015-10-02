using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
	[TestFixture]
	public class PersonAssemblerTest
	{
		private PersonAssembler _target;
		private IAssembler<IWorkflowControlSet, WorkflowControlSetDto> _workflowControlSetAssembler;
		private IPersonRepository _personRepository;
		private IPersonAccountUpdater _personAccountUpdater;
		private ITenantPeopleLoader _tenantPeopleLoader;

		[SetUp]
		public void Setup()
		{
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_workflowControlSetAssembler = MockRepository.GenerateMock<IAssembler<IWorkflowControlSet, WorkflowControlSetDto>>();
			_personAccountUpdater = MockRepository.GenerateMock<IPersonAccountUpdater>();
			_tenantPeopleLoader = MockRepository.GenerateMock<ITenantPeopleLoader>();
			_target = new PersonAssembler(_personRepository, _workflowControlSetAssembler, _personAccountUpdater, _tenantPeopleLoader);
		}

		private IPerson CreatePerson(bool createWorkflowControlSet)
		{
			var person = PersonFactory.CreatePerson("testuser", "123");
			person.SetId(Guid.NewGuid());
			person.PermissionInformation.SetCulture(new CultureInfo(1053));
			person.PermissionInformation.SetUICulture(new CultureInfo(1025));
			person.Name = new Name("aaa", "bbb");
			person.Email = "email";
			person.EmploymentNumber = "email";
			person.Note = "A very good agent";
			person.TerminatePerson(new DateOnly(2011, 8, 20), new PersonAccountUpdaterDummy());
			((Person)person).SetDeleted();


			if (createWorkflowControlSet)
			{
				var workflowControlSet = new WorkflowControlSet("Controlset");
				person.WorkflowControlSet = workflowControlSet;
			}

			var period = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1));
			period.SetParent(person);
			person.AddPersonPeriod(period);

			return person;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyDomainEntityToDto()
		{
			var person = CreatePerson(true);
			_workflowControlSetAssembler.Stub(x => x.DomainEntityToDto(person.WorkflowControlSet))
					 .Return(new WorkflowControlSetDto { Id = person.WorkflowControlSet.Id })
					 .IgnoreArguments()
					 .Repeat.Once();

			var personDto = _target.DomainEntityToDto(person);
			_tenantPeopleLoader.AssertWasCalled(x => x.FillDtosWithLogonInfo(Arg<List<PersonDto>>.Is.Anything));

			Assert.AreEqual(person.Id, personDto.Id);
			Assert.AreEqual(person.Name.ToString(), personDto.Name);
			Assert.AreEqual(person.Name.FirstName, personDto.FirstName);
			Assert.AreEqual(person.Name.LastName, personDto.LastName);
			Assert.AreEqual(person.Email, personDto.Email);
			Assert.AreEqual(person.EmploymentNumber, personDto.EmploymentNumber);
			Assert.AreEqual(person.PermissionInformation.CultureLCID(), personDto.CultureLanguageId);
			Assert.AreEqual(person.PermissionInformation.UICultureLCID(), personDto.UICultureLanguageId);
			Assert.AreEqual(person.PermissionInformation.DefaultTimeZone().Id, personDto.TimeZoneId);
			Assert.AreEqual(person.PersonPeriodCollection.Count(), personDto.PersonPeriodCollection.Count);
			Assert.AreEqual(person.WorkflowControlSet.Id, personDto.WorkflowControlSet.Id);
			Assert.AreEqual(person.Note, personDto.Note);
			Assert.AreEqual(person.TerminalDate.Value.Date, personDto.TerminationDate.DateTime);
			Assert.AreEqual(person.FirstDayOfWeek, personDto.FirstDayOfWeek);
			Assert.AreEqual(((Person)person).IsDeleted, personDto.IsDeleted);
		}

		[Test]
		public void ShouldMapDomainEntityWithoutWorkflowControlSetToDto()
		{
			var person = CreatePerson(false);
			var personDto = _target.DomainEntityToDto(person);

			Assert.That(personDto.WorkflowControlSet, Is.Null);
		}

		[Test]
		public void VerifyCanTransformToDomainObject()
		{
			var person = CreatePerson(true);
			_personRepository.Stub(x => x.Get(person.Id.Value)).Return(person);

			_workflowControlSetAssembler.Stub(x => x.DomainEntityToDto(person.WorkflowControlSet))
				 .Return(new WorkflowControlSetDto { Id = person.WorkflowControlSet.Id })
				 .IgnoreArguments()
				 .Repeat.Any();


			var personDto = _target.DomainEntityToDto(person);
			personDto.CultureLanguageId = 1053;
			personDto.UICultureLanguageId = 1025;
			personDto.FirstName = "aaa";
			personDto.LastName = "bbb";
			personDto.IsDeleted = true;

			var personDo = _target.DtoToDomainEntity(personDto);

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

			var personDo = _target.DtoToDomainEntity(personDto);

			Assert.AreEqual(personDto.Id, personDo.Id);
			Assert.AreEqual(personDto.FirstName, personDo.Name.FirstName);
			Assert.AreEqual(personDto.LastName, personDo.Name.LastName);
			Assert.AreEqual(personDto.Email, personDo.Email);
			Assert.AreEqual(personDto.EmploymentNumber, personDo.EmploymentNumber);
			Assert.AreEqual(personDto.UICultureLanguageId, personDo.PermissionInformation.UICultureLCID());
			Assert.AreEqual(personDto.CultureLanguageId, personDo.PermissionInformation.CultureLCID());
			Assert.AreEqual(personDto.CultureLanguageId, personDo.PermissionInformation.CultureLCID());
			Assert.AreEqual(personDto.UICultureLanguageId, personDo.PermissionInformation.UICultureLCID());
			Assert.AreEqual(personDto.IsDeleted, ((Person)personDo).IsDeleted);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void VerifyCannotAddPersonWithoutName()
		{
			string timeZone = "W. Europe Standard Time";
			int culture = 1053;

			var personDto = new PersonDto { TimeZoneId = timeZone, CultureLanguageId = culture };

			_target.DtoToDomainEntity(personDto);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void VerifyCannotUpdatePersonWithoutName()
		{
			var personId = Guid.NewGuid();
			_personRepository.Stub(x => x.Get(personId)).Return(PersonFactory.CreatePerson());

			string timeZone = "W. Europe Standard Time";
			int culture = 1053;

			var personDto = new PersonDto { Id = personId };
			personDto.TimeZoneId = timeZone;
			personDto.CultureLanguageId = culture;

			_target.EnableSaveOrUpdate = true;
			_target.DtoToDomainEntity(personDto);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void VerifyCannotAddPersonWithoutTimeZone()
		{
			string firstName = "Kalle";
			string lastName = "Kula";
			int culture = 1053;

			var personDto = new PersonDto { FirstName = firstName, LastName = lastName, CultureLanguageId = culture };

			_target.DtoToDomainEntity(personDto);
		}

		[Test]
		public void ShouldUseDummyAccountUpdaterWhenNewPerson()
		{
			var personDto = new PersonDto
			{
				FirstName = "Personliga",
				LastName = "Person",
				CultureLanguageId = 1053,
				TimeZoneId = "W. Europe Standard Time",
				TerminationDate = new DateOnlyDto(2014, 4, 11)
			};

			_target.DtoToDomainEntity(personDto);
			_personAccountUpdater.AssertWasNotCalled(x => x.Update(null), y => y.IgnoreArguments());
		}
	}
}

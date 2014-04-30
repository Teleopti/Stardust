using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonAssemblerTest
    {
        private PersonAssembler _target;
        private IAssembler<IWorkflowControlSet,WorkflowControlSetDto> _workflowControlSetAssembler;
        private MockRepository _mocks;
        private IPersonRepository _personRepository;
        private IPersonAccountUpdater _personAccountUpdater;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _personRepository = _mocks.StrictMock<IPersonRepository>();
            _workflowControlSetAssembler = _mocks.StrictMock<IAssembler<IWorkflowControlSet, WorkflowControlSetDto>>();
				_personAccountUpdater = MockRepository.GenerateMock<IPersonAccountUpdater>();
            _target = new PersonAssembler(_personRepository, _workflowControlSetAssembler, _personAccountUpdater);
        }

        private IPerson CreatePerson(bool createWorkflowControlSet)
        {
            var person = PersonFactory.CreatePersonWithBasicPermissionInfo("testuser", "123");
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.SetCulture(new CultureInfo(1053));
            person.PermissionInformation.SetUICulture(new CultureInfo(1025));
            person.Name = new Name("aaa", "bbb");
            person.Email = "email";
            person.EmploymentNumber = "email";
            person.Note = "A very good agent";
            person.TerminatePerson(new DateOnly(2011, 8, 20), new PersonAccountUpdaterDummy());
            ((Person) person).SetDeleted();
            
            
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
	        person.AuthenticationInfo = new AuthenticationInfo {Identity = @"DOM\AIN"};
            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                       {ApplicationLogOnName = "App", Password = "Pass"};
            using (_mocks.Record())
            {
                Expect.Call(_workflowControlSetAssembler.DomainEntityToDto(person.WorkflowControlSet))
                    .Return(new WorkflowControlSetDto {Id = person.WorkflowControlSet.Id})
                    .IgnoreArguments()
                    .Repeat.Once();
            }

            _mocks.ReplayAll();

            var personDto = _target.DomainEntityToDto(person);

            _mocks.VerifyAll();

            Assert.AreEqual(person.Id, personDto.Id);
            Assert.AreEqual(person.Name.ToString(), personDto.Name);
            Assert.AreEqual(person.Name.FirstName, personDto.FirstName);
            Assert.AreEqual(person.Name.LastName, personDto.LastName);
            Assert.AreEqual(person.Email, personDto.Email);
            Assert.AreEqual(person.EmploymentNumber, personDto.EmploymentNumber);
            Assert.AreEqual(person.PermissionInformation.CultureLCID(), personDto.CultureLanguageId);
            Assert.AreEqual(person.PermissionInformation.UICultureLCID(), personDto.UICultureLanguageId);
            Assert.AreEqual(person.PermissionInformation.DefaultTimeZone().Id, personDto.TimeZoneId);
            Assert.AreEqual(person.ApplicationAuthenticationInfo.ApplicationLogOnName,personDto.ApplicationLogOnName);
	        Assert.AreEqual(person.AuthenticationInfo.Identity, personDto.WindowsDomain + @"\" + personDto.WindowsLogOnName);
            Assert.AreEqual(person.PersonPeriodCollection.Count(), personDto.PersonPeriodCollection.Count);
            Assert.AreEqual(person.WorkflowControlSet.Id, personDto.WorkflowControlSet.Id);
            Assert.AreEqual(person.Note, personDto.Note);
            Assert.AreEqual(person.TerminalDate.Value.Date, personDto.TerminationDate.DateTime);
            Assert.AreEqual(((Person)person).IsDeleted, personDto.IsDeleted);
        }

        [Test]
        public void ShouldMapDomainEntityWithoutWorkflowControlSetToDto()
        {
			var person = CreatePerson(false);

	        _mocks.Record();

            _mocks.ReplayAll();

            var personDto = _target.DomainEntityToDto(person);

            Assert.That(personDto.WorkflowControlSet, Is.Null);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanTransformToDomainObject()
        {
            var person = CreatePerson(true);
	        person.AuthenticationInfo = new AuthenticationInfo {Identity = @"DOMATRIX\DONNA"};
            using (_mocks.Record())
            {
                Expect.Call(_personRepository.Get(person.Id.Value))
                    .Return(person);

                Expect.Call(_workflowControlSetAssembler.DomainEntityToDto(person.WorkflowControlSet))
                    .Return(new WorkflowControlSetDto {Id = person.WorkflowControlSet.Id})
                    .IgnoreArguments()
                    .Repeat.Any();
            }

            _mocks.ReplayAll();

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
            Assert.AreEqual(personDto.ApplicationLogOnName, "");
            Assert.AreEqual(personDto.ApplicationLogOnPassword, "");
			Assert.AreEqual(personDto.WindowsDomain + @"\" + personDto.WindowsLogOnName, personDo.AuthenticationInfo.Identity);
            Assert.AreEqual(personDto.Note, personDo.Note);
            Assert.AreEqual(personDto.TimeZoneId, personDo.PermissionInformation.DefaultTimeZone().DisplayName);
            Assert.AreEqual(personDto.TerminationDate.DateTime, personDo.TerminalDate.Value.Date);
            Assert.AreEqual(personDto.Note, personDo.Note);
            Assert.AreEqual(personDto.IsDeleted, ((Person)personDo).IsDeleted);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanTransformToDomainObjectFromNewPersonDto()
        {
            var personDto = new PersonDto();
            personDto.WindowsLogOnName = "kallekWindows";
            personDto.ApplicationLogOnName = "kallek";
            personDto.ApplicationLogOnPassword = "mammamu";
            personDto.Email = "kalle.kula@teleopti.com";
            personDto.EmploymentNumber = "abc123";
            personDto.FirstName = "Kalle";
            personDto.LastName = "Kula";
            personDto.TimeZoneId = "W. Europe Standard Time";
            personDto.WorkflowControlSet = null;
            personDto.CultureLanguageId = 1053;
            personDto.UICultureLanguageId = 1025;
            personDto.WindowsDomain = "toptinet";
            personDto.Note = "Moahaha";
            personDto.IsDeleted = true;

            var personDo = _target.DtoToDomainEntity(personDto);

            Assert.AreEqual(personDto.Id, personDo.Id);
            Assert.AreEqual(personDto.FirstName, personDo.Name.FirstName);
            Assert.AreEqual(personDto.LastName, personDo.Name.LastName);
            Assert.AreEqual(personDto.Email, personDo.Email);
            Assert.AreEqual(personDto.EmploymentNumber, personDo.EmploymentNumber);
            Assert.AreEqual(personDto.UICultureLanguageId, personDo.PermissionInformation.UICultureLCID());
            Assert.AreEqual(personDto.CultureLanguageId, personDo.PermissionInformation.CultureLCID());
            //Because it is deleted
            Assert.That(personDo.AuthenticationInfo, Is.Null);
            Assert.That( personDo.ApplicationAuthenticationInfo, Is.Null);
            Assert.AreEqual(personDto.CultureLanguageId, personDo.PermissionInformation.CultureLCID());
            Assert.AreEqual(personDto.UICultureLanguageId, personDo.PermissionInformation.UICultureLCID());
            Assert.AreEqual(personDto.IsDeleted, ((Person)personDo).IsDeleted);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyCannotAddPersonWithoutName()
        {
            string timeZone = "W. Europe Standard Time";
            int culture = 1053;

            var personDto = new PersonDto();
            personDto.TimeZoneId = timeZone;
            personDto.CultureLanguageId = culture;

            _target.DtoToDomainEntity(personDto);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void VerifyCannotUpdatePersonWithoutName()
        {
            var personId = Guid.NewGuid();
            using (_mocks.Record())
            {
                Expect.Call(_personRepository.Get(personId))
                    .Return(PersonFactory.CreatePerson());
            }
            string timeZone = "W. Europe Standard Time";
            int culture = 1053;

            var personDto = new PersonDto{Id=personId};
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

            var personDto = new PersonDto();
            personDto.FirstName = firstName;
            personDto.LastName = lastName;
            personDto.CultureLanguageId = culture;
            
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

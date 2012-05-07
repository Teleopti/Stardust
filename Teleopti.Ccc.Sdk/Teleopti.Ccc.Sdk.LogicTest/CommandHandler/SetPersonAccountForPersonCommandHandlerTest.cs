using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class SetPersonAccountForPersonCommandHandlerTest
    {
        private MockRepository _mock;
        private IRepositoryFactory _repositoryFactory;
        private IScenarioRepository _scenarioRepository;
        private IPersonRepository _personRepository;
        private IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private IAbsenceRepository _absenceRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private SetPersonAccountForPersonCommandHandler _target;


        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _repositoryFactory = _mock.StrictMock<IRepositoryFactory>();
            _scenarioRepository = _mock.StrictMock<IScenarioRepository>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _personAbsenceAccountRepository = _mock.StrictMock<IPersonAbsenceAccountRepository>();
            _absenceRepository = _mock.StrictMock<IAbsenceRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _target = new SetPersonAccountForPersonCommandHandler(_repositoryFactory,_scenarioRepository,_personRepository,_personAbsenceAccountRepository,_absenceRepository,_unitOfWorkFactory);
        }

        [Test]
        public void ShouldSetPersonAccountForPerson()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var absence = AbsenceFactory.CreateAbsence("test absence");
            absence.SetId(Guid.NewGuid());
            
            var startDate = new DateTime(2012, 1, 1,0,0,0,DateTimeKind.Utc);
            var dateOnly = new DateOnly(startDate);
            var dateOnlydto = new DateOnlyDto(new DateOnly(startDate));

            var personAccounts = _mock.StrictMock<IPersonAccountCollection>();
            var account = _mock.StrictMock<IAccount>();

            var setPersonAccountForPersonCommandDto = new SetPersonAccountForPersonCommandDto();
            setPersonAccountForPersonCommandDto.PersonId = person.Id.GetValueOrDefault();
            setPersonAccountForPersonCommandDto.AbsenceId = absence.Id.GetValueOrDefault();
            setPersonAccountForPersonCommandDto.DateFrom = dateOnlydto;
            
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Get(setPersonAccountForPersonCommandDto.PersonId)).Return(person);
                Expect.Call(_absenceRepository.Get(setPersonAccountForPersonCommandDto.AbsenceId)).Return(absence);
                Expect.Call(_personAbsenceAccountRepository.Find(person)).Return(personAccounts);
                Expect.Call(account.StartDate).Return(dateOnly);
                Expect.Call(personAccounts.Find(absence, dateOnly )).Return(account);
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
            }
            using(_mock.Playback())
            {
                _target.Handle(setPersonAccountForPersonCommandDto);
            }
        }
    }
}

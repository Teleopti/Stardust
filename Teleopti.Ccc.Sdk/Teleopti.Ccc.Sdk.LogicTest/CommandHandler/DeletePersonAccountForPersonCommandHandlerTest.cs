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
    public class DeletePersonAccountForPersonCommandHandlerTest
    {
        private MockRepository _mock;
        private IPersonRepository _personRepository;
        private IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private IAbsenceRepository _absenceRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private DeletePersonAccountForPersonCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _personAbsenceAccountRepository = _mock.StrictMock<IPersonAbsenceAccountRepository>();
            _absenceRepository = _mock.StrictMock<IAbsenceRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _target = new DeletePersonAccountForPersonCommandHandler(_personRepository,_personAbsenceAccountRepository,_absenceRepository,_unitOfWorkFactory);
        }

        [Test]
        public void DeletePersonAccountForPersonSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            IPerson person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());
            IAbsence absence = AbsenceFactory.CreateAbsence("Sick");
            absence.SetId(Guid.NewGuid());
            var startDate = new DateOnly(2012, 1, 1);
            var dateOnydto = new DateOnlyDto(startDate);
            var personAccounts = _mock.StrictMock<IPersonAccountCollection>();
            var account = _mock.StrictMock<IAccount>();

            var deletePersonAccountForPersonCommandDto = new DeletePersonAccountForPersonCommandDto
                                                             {
                                                                 AbsenceId = absence.Id.GetValueOrDefault(),
                                                                 DateFrom = dateOnydto,
                                                                 PersonId = person.Id.GetValueOrDefault()
                                                             };

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRepository.Get(deletePersonAccountForPersonCommandDto.PersonId)).Return(person);
                Expect.Call(_absenceRepository.Get(deletePersonAccountForPersonCommandDto.AbsenceId)).Return(absence);
                Expect.Call(_personAbsenceAccountRepository.Find(person)).Return(personAccounts);
                Expect.Call(personAccounts.Find(absence, startDate)).Return(account);
                Expect.Call(() => personAccounts.Remove(account));
            }
            using(_mock.Playback())
            {
                _target.Handle(deletePersonAccountForPersonCommandDto);
                
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
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
        private ICurrentScenario _scenarioRepository;
        private IPersonRepository _personRepository;
        private IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private IAbsenceRepository _absenceRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private SetPersonAccountForPersonCommandHandler _target;
        private IPerson _person;
        private IAbsence _absence;
        private static DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateOnly _dateOnly = new DateOnly(_startDate);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = new DateOnly(_startDate) };
        private SetPersonAccountForPersonCommandDto _setPersonAccountForPersonCommandDto;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _repositoryFactory = _mock.DynamicMock<IRepositoryFactory>();
			_scenarioRepository = _mock.StrictMock<ICurrentScenario>();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _personAbsenceAccountRepository = _mock.StrictMock<IPersonAbsenceAccountRepository>();
            _absenceRepository = _mock.StrictMock<IAbsenceRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _target = new SetPersonAccountForPersonCommandHandler(_repositoryFactory,_scenarioRepository,_personRepository,_personAbsenceAccountRepository,_absenceRepository,_currentUnitOfWorkFactory);

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());

            _absence = AbsenceFactory.CreateAbsence("test absence");
            _absence.SetId(Guid.NewGuid());

            _setPersonAccountForPersonCommandDto = new SetPersonAccountForPersonCommandDto
                                                       {
                                                           PersonId = _person.Id.GetValueOrDefault(),
                                                           AbsenceId = _absence.Id.GetValueOrDefault(),
                                                           DateFrom = _dateOnlydto,
                                                           Accrued = TimeSpan.FromMinutes(10).Ticks,
                                                           BalanceIn = TimeSpan.FromMinutes(11).Ticks,
                                                           Extra = TimeSpan.FromMinutes(12).Ticks
                                                       };
        }

        [Test]
        public void ShouldSetPersonAccountForPerson()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var personAccounts = _mock.StrictMock<IPersonAccountCollection>();
            var account = _mock.StrictMock<IAccount>();
            
            using(_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_setPersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_setPersonAccountForPersonCommandDto.AbsenceId)).Return(_absence);
                Expect.Call(_personAbsenceAccountRepository.Find(_person)).Return(personAccounts);
                Expect.Call(account.StartDate).Return(_dateOnly);
                Expect.Call(personAccounts.Find(_absence, _dateOnly )).Return(account);
                Expect.Call(unitOfWork.PersistAll());
                Expect.Call(account.Accrued = TimeSpan.FromMinutes(10));
                Expect.Call(account.BalanceIn = TimeSpan.FromMinutes(11));
                Expect.Call(account.Extra = TimeSpan.FromMinutes(12));
            }
            using(_mock.Playback())
            {
                _target.Handle(_setPersonAccountForPersonCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfPersonDoesNotFound()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_setPersonAccountForPersonCommandDto.PersonId)).Return(null);
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(_setPersonAccountForPersonCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfAbsenceDoesNotFound()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_setPersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_setPersonAccountForPersonCommandDto.AbsenceId)).Return(null);
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(_setPersonAccountForPersonCommandDto);
            }
        }

        [Test]
        public void ShouldCreateNewPersonAccountIfPersonAccountIsNull()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var personAccounts = _mock.StrictMock<IPersonAccountCollection>();
            var account = _mock.DynamicMock<IAccount>();
            var tracker = _mock.DynamicMock<ITracker>();
             _absence.Tracker = tracker;

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_setPersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_setPersonAccountForPersonCommandDto.AbsenceId)).Return(_absence);
                Expect.Call(_personAbsenceAccountRepository.Find(_person)).Return(personAccounts);
                Expect.Call(personAccounts.Find(_absence, _dateOnly)).Return(null);
                Expect.Call(() => personAccounts.Add(_absence, account)).IgnoreArguments();
                Expect.Call(() => _personAbsenceAccountRepository.AddRange(new List<IPersonAbsenceAccount>())).
                    IgnoreArguments();
                Expect.Call(() => _scenarioRepository.Current());
                Expect.Call(personAccounts.PersonAbsenceAccounts()).Return(null);
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(tracker.CreatePersonAccount(_dateOnly)).Return(account);
            }
            using (_mock.Playback())
            {
                _target.Handle(_setPersonAccountForPersonCommandDto);
            }
        }

        [Test]
        public void ShouldSetPersonAccount()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var personAccounts = _mock.StrictMock<IPersonAccountCollection>();
            var account = _mock.DynamicMock<IAccount>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_setPersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_setPersonAccountForPersonCommandDto.AbsenceId)).Return(_absence);
                Expect.Call(_personAbsenceAccountRepository.Find(_person)).Return(personAccounts);
                Expect.Call(account.StartDate).Return(_dateOnly);
                Expect.Call(personAccounts.Find(_absence, _dateOnly)).Return(account);
                Expect.Call(() => unitOfWork.PersistAll());
            }
            using (_mock.Playback())
            {
                _target.Handle(_setPersonAccountForPersonCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfNotPermitted()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_setPersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_setPersonAccountForPersonCommandDto.AbsenceId)).Return(_absence);
                
            }
            using (_mock.Playback())
            {
                using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
                {
                    _target.Handle(_setPersonAccountForPersonCommandDto);
                }
            }
        }
    }
}

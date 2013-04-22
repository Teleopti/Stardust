﻿using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
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
    public class DeletePersonAccountForPersonCommandHandlerTest
    {
        private MockRepository _mock;
        private IPersonRepository _personRepository;
        private IPersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private IAbsenceRepository _absenceRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private DeletePersonAccountForPersonCommandHandler _target;
        private IPerson _person;
        private IAbsence _absence;
        private static DateOnly _startDate = new DateOnly(2012, 1, 1);
		private readonly DateOnlyDto _dateOnydto = new DateOnlyDto { DateTime = _startDate };
        private DeletePersonAccountForPersonCommandDto _deletePersonAccountForPersonCommandDto;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;


        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _personAbsenceAccountRepository = _mock.StrictMock<IPersonAbsenceAccountRepository>();
            _absenceRepository = _mock.StrictMock<IAbsenceRepository>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _target = new DeletePersonAccountForPersonCommandHandler(_personRepository,_personAbsenceAccountRepository,_absenceRepository,_currentUnitOfWorkFactory);

            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());
            _absence = AbsenceFactory.CreateAbsence("Sick");
            _absence.SetId(Guid.NewGuid());

            _deletePersonAccountForPersonCommandDto = new DeletePersonAccountForPersonCommandDto
            {
                AbsenceId = _absence.Id.GetValueOrDefault(),
                DateFrom = _dateOnydto,
                PersonId = _person.Id.GetValueOrDefault()
            };
        }

        [Test]
        public void DeletePersonAccountForPersonSuccessfully()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            var personAccounts = _mock.StrictMock<IPersonAccountCollection>();
            var account = _mock.StrictMock<IAccount>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_deletePersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_deletePersonAccountForPersonCommandDto.AbsenceId)).Return(_absence);
                Expect.Call(_personAbsenceAccountRepository.Find(_person)).Return(personAccounts);
                Expect.Call(personAccounts.Find(_absence, _startDate)).Return(account);
                Expect.Call(() => personAccounts.Remove(account));
            }
            using(_mock.Playback())
            {
                _target.Handle(_deletePersonAccountForPersonCommandDto);
                
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfPersonDoesNotExists()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_deletePersonAccountForPersonCommandDto.PersonId)).Return(null);
            }
            using (_mock.Playback())
            {
                _target.Handle(_deletePersonAccountForPersonCommandDto);

            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfAbsenceDoesNotExists()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_deletePersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_deletePersonAccountForPersonCommandDto.AbsenceId)).Return(null);
            }
            using (_mock.Playback())
            {
                _target.Handle(_deletePersonAccountForPersonCommandDto);

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
                Expect.Call(_personRepository.Get(_deletePersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_deletePersonAccountForPersonCommandDto.AbsenceId)).Return(_absence);
            }
            using (_mock.Playback())
            {
                using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
                {
                    _target.Handle(_deletePersonAccountForPersonCommandDto);
                }
            }
        }
    }
}

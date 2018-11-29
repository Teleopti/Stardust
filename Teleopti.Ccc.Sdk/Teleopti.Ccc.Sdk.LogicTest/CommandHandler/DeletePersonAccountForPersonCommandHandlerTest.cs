using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;


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
        private static readonly DateOnly _startDate = new DateOnly(2012, 1, 1);
		private readonly DateOnlyDto _dateOnydto = new DateOnlyDto { DateTime = _startDate.Date };
        private DeletePersonAccountForPersonCommandDto _deletePersonAccountForPersonCommandDto;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private ITraceableRefreshService _traceableRefreshService;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personRepository = _mock.StrictMock<IPersonRepository>();
            _personAbsenceAccountRepository = _mock.StrictMock<IPersonAbsenceAccountRepository>();
            _absenceRepository = _mock.StrictMock<IAbsenceRepository>();
			_traceableRefreshService = _mock.DynamicMock<ITraceableRefreshService>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _target = new DeletePersonAccountForPersonCommandHandler(_traceableRefreshService, _personRepository,_personAbsenceAccountRepository,_absenceRepository, _currentUnitOfWorkFactory);

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
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_deletePersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_deletePersonAccountForPersonCommandDto.AbsenceId)).Return(_absence);
                Expect.Call(_personAbsenceAccountRepository.Find(_person)).Return(personAccounts);
                Expect.Call(personAccounts.Find(_absence, _startDate)).Return(account).Repeat.Twice();
                Expect.Call(() => personAccounts.Remove(account));
                Expect.Call(() => _traceableRefreshService.Refresh(account));
            }
            using(_mock.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					_target.Handle(_deletePersonAccountForPersonCommandDto);
				}
			}
        }

        [Test]
        public void ShouldThrowExceptionIfPersonDoesNotExists()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_deletePersonAccountForPersonCommandDto.PersonId)).Return(null);
            }
            using (_mock.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					Assert.Throws<FaultException>(() => _target.Handle(_deletePersonAccountForPersonCommandDto));
				}
			}
        }

        [Test]
        public void ShouldThrowExceptionIfAbsenceDoesNotExists()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_deletePersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_deletePersonAccountForPersonCommandDto.AbsenceId)).Return(null);
            }
            using (_mock.Playback())
			{
				using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
				{
					Assert.Throws<FaultException>(() => _target.Handle(_deletePersonAccountForPersonCommandDto));
				}
			}
        }

        [Test]
        public void ShouldThrowExceptionIfNotPermitted()
        {
            var unitOfWork = _mock.DynamicMock<IUnitOfWork>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRepository.Get(_deletePersonAccountForPersonCommandDto.PersonId)).Return(_person);
                Expect.Call(_absenceRepository.Get(_deletePersonAccountForPersonCommandDto.AbsenceId)).Return(_absence);
            }
            using (_mock.Playback())
            {
                using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
                {
                    Assert.Throws<FaultException>(() => _target.Handle(_deletePersonAccountForPersonCommandDto));
                }
            }
        }
    }
}

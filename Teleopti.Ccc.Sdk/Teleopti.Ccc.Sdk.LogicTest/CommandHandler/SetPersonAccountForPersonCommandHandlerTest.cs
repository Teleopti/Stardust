using System;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class SetPersonAccountForPersonCommandHandlerTest
    {
        private FakePersonRepository _personRepository;
        private FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;
        private FakeAbsenceRepository _absenceRepository;
        private SetPersonAccountForPersonCommandHandler _target;
        private IPerson _person;
        private IAbsence _absence;
        private static readonly DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateOnly _dateOnly = new DateOnly(_startDate);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = _startDate.Date };
        private SetPersonAccountForPersonCommandDto _setPersonAccountForPersonCommandDto;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
        private ITraceableRefreshService _tracesableRefreshService;

        [SetUp]
        public void Setup()
        {
            _personRepository = new FakePersonRepository(new FakeStorage());
            _personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
            _absenceRepository = new FakeAbsenceRepository();
            _currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
            _tracesableRefreshService = new TraceableRefreshService(new FakeCurrentScenario(), new FakeScheduleStorage());
            _target = new SetPersonAccountForPersonCommandHandler(_personRepository,_personAbsenceAccountRepository,_absenceRepository,_currentUnitOfWorkFactory,_tracesableRefreshService);

            _person = PersonFactory.CreatePerson("test").WithId();
            _personRepository.Add(_person);

            _absence = AbsenceFactory.CreateAbsence("test absence").WithId();
            _absenceRepository.Add(_absence);

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
		    var personAbsenceAccount = new PersonAbsenceAccount(_person, _absence);
		    var accountDay = new AccountDay(_dateOnly);
		    personAbsenceAccount.Add(accountDay);
		    _personAbsenceAccountRepository.Add(personAbsenceAccount);

		    _target.Handle(_setPersonAccountForPersonCommandDto);

		    accountDay.Accrued.Should().Be.EqualTo(TimeSpan.FromMinutes(10));
		    accountDay.BalanceIn.Should().Be.EqualTo(TimeSpan.FromMinutes(11));
		    accountDay.Extra.Should().Be.EqualTo(TimeSpan.FromMinutes(12));
	    }

	    [Test]
	    public void ShouldThrowExceptionIfPersonDoesNotFound()
	    {
		    _personRepository.Remove(_person);
		    Assert.Throws<FaultException>(() => _target.Handle(_setPersonAccountForPersonCommandDto));
	    }

	    [Test]
	    public void ShouldThrowExceptionIfAbsenceDoesNotFound()
	    {
		    _absenceRepository.Remove(_absence);
		    Assert.Throws<FaultException>(() => _target.Handle(_setPersonAccountForPersonCommandDto));
	    }

	    [Test]
	    public void ShouldThrowExceptionIfNotPermitted()
	    {
		    using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
		    {
			    Assert.Throws<FaultException>(() => _target.Handle(_setPersonAccountForPersonCommandDto));
		    }
	    }
    }
}

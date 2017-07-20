using System;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

	public class Bug44502Test
	{
		[Test]
		public void ShouldNotHappen()
		{
			DateTime startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateOnly dateOnly = new DateOnly(startDate);
			DateOnlyDto dateOnlydto1 = new DateOnlyDto { DateTime = startDate.Date.AddDays(-10) };
			DateOnlyDto dateOnlydto2 = new DateOnlyDto { DateTime = startDate.Date };
			DateOnlyDto dateOnlydto3 = new DateOnlyDto { DateTime = startDate.Date.AddDays(10) };

			var personRepository = new FakePersonRepository(new FakeStorage());
			var personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			var absenceRepository = new FakeAbsenceRepository();
			var currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
			var currentScenario = new FakeCurrentScenario();
			var scheduleStorage = new FakeScheduleStorage();
			var tracesableRefreshService = new TraceableRefreshService(currentScenario, scheduleStorage);
			
			var person = PersonFactory.CreatePerson("test").WithId();
			personRepository.Add(person);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			absence.Priority = 100;
			absence.InContractTime = true;
			absence.Tracker = Tracker.CreateTimeTracker();
			var absenceSecond = AbsenceFactory.CreateAbsence("test absence").WithId();
			absenceSecond.Priority = 100;
			absenceSecond.InContractTime = true;

			absenceRepository.Add(absence);
			absenceRepository.Add(absenceSecond);

			var assignment = new PersonAssignment(person, currentScenario.Current(), dateOnly);
			var activity = ActivityFactory.CreateActivity("Phone");
			assignment.AddActivity(activity,new DateTimePeriod(startDate.AddHours(8),startDate.AddHours(17)));
			scheduleStorage.Add(assignment);

			var assignment2 = new PersonAssignment(person, currentScenario.Current(), dateOnly.AddDays(1));
			assignment2.AddActivity(activity, new DateTimePeriod(startDate.AddDays(1).AddHours(8), startDate.AddDays(1).AddHours(17)));
			scheduleStorage.Add(assignment2);

			var firstAbsence = new PersonAbsence(person, currentScenario.Current(),
				new AbsenceLayer(absence, new DateTimePeriod(startDate.AddHours(8), startDate.AddHours(17))));
			scheduleStorage.Add(firstAbsence);

			var firstAbsence2 = new PersonAbsence(person, currentScenario.Current(),
				new AbsenceLayer(absence, new DateTimePeriod(startDate.AddDays(1).AddHours(8), startDate.AddDays(1).AddHours(17))));
			scheduleStorage.Add(firstAbsence2);

			var secondAbsence = new PersonAbsence(person, currentScenario.Current(),
				new AbsenceLayer(absenceSecond, new DateTimePeriod(startDate.AddHours(8), startDate.AddHours(17))));
			scheduleStorage.Add(secondAbsence);

			var target = new SetPersonAccountForPersonCommandHandler(personRepository, personAbsenceAccountRepository, absenceRepository, currentUnitOfWorkFactory, tracesableRefreshService);
			target.Handle(new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = dateOnlydto1,
				Accrued = 0L,
				BalanceIn = 0L,
				Extra = 0L
			});
			target.Handle(new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = dateOnlydto2,
				Accrued = TimeSpan.FromHours(10).Ticks,
				BalanceIn = 0L,
				Extra = 0L
			});
			target.Handle(new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = dateOnlydto3,
				Accrued = 0L,
				BalanceIn = 0L,
				Extra = 0L
			});

			personAbsenceAccountRepository.Find(person).Find(absence, dateOnly).LatestCalculatedBalance.Should().Be
				.EqualTo(TimeSpan.FromHours(9));
		}
	}
}

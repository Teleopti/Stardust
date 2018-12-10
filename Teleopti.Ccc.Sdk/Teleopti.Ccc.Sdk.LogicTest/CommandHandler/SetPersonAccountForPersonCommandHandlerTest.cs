using System;
using System.ServiceModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
	[DomainTest]
    public class SetPersonAccountForPersonCommandHandlerTest : IExtendSystem
    {
        public FakePersonRepository PersonRepository;
        public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
        public FakeAbsenceRepository AbsenceRepository;
        public SetPersonAccountForPersonCommandHandler Target;
		
        private static readonly DateTime _startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly DateOnly _dateOnly = new DateOnly(_startDate);
		private readonly DateOnlyDto _dateOnlydto = new DateOnlyDto { DateTime = _startDate.Date };
        
	    [Test]
	    public void ShouldSetPersonAccountForPerson()
	    {
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Add(person);

			var absence = AbsenceFactory.CreateAbsence("test absence").WithId();
			AbsenceRepository.Add(absence);

			var setPersonAccountForPersonCommandDto = new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = _dateOnlydto,
				Accrued = TimeSpan.FromMinutes(10).Ticks,
				BalanceIn = TimeSpan.FromMinutes(11).Ticks,
				Extra = TimeSpan.FromMinutes(12).Ticks
			};

			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
		    var accountDay = new AccountDay(_dateOnly);
		    personAbsenceAccount.Add(accountDay);
		    PersonAbsenceAccountRepository.Add(personAbsenceAccount);

		    Target.Handle(setPersonAccountForPersonCommandDto);

		    accountDay.Accrued.Should().Be.EqualTo(TimeSpan.FromMinutes(10));
		    accountDay.BalanceIn.Should().Be.EqualTo(TimeSpan.FromMinutes(11));
		    accountDay.Extra.Should().Be.EqualTo(TimeSpan.FromMinutes(12));
	    }

	    [Test]
	    public void ShouldThrowExceptionIfPersonDoesNotFound()
		{
			var absence = AbsenceFactory.CreateAbsence("test absence").WithId();
			AbsenceRepository.Add(absence);

			var setPersonAccountForPersonCommandDto = new SetPersonAccountForPersonCommandDto
			{
				PersonId = Guid.NewGuid(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = _dateOnlydto,
				Accrued = TimeSpan.FromMinutes(10).Ticks,
				BalanceIn = TimeSpan.FromMinutes(11).Ticks,
				Extra = TimeSpan.FromMinutes(12).Ticks
			};
			
		    Assert.Throws<FaultException>(() => Target.Handle(setPersonAccountForPersonCommandDto));
	    }

	    [Test]
	    public void ShouldThrowExceptionIfAbsenceDoesNotFound()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Add(person);
			
			var setPersonAccountForPersonCommandDto = new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = Guid.NewGuid(),
				DateFrom = _dateOnlydto,
				Accrued = TimeSpan.FromMinutes(10).Ticks,
				BalanceIn = TimeSpan.FromMinutes(11).Ticks,
				Extra = TimeSpan.FromMinutes(12).Ticks
			};
			
		    Assert.Throws<FaultException>(() => Target.Handle(setPersonAccountForPersonCommandDto));
	    }

	    [Test]
	    public void ShouldThrowExceptionIfNotPermitted()
		{
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Add(person);

			var absence = AbsenceFactory.CreateAbsence("test absence").WithId();
			AbsenceRepository.Add(absence);

			var setPersonAccountForPersonCommandDto = new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = _dateOnlydto,
				Accrued = TimeSpan.FromMinutes(10).Ticks,
				BalanceIn = TimeSpan.FromMinutes(11).Ticks,
				Extra = TimeSpan.FromMinutes(12).Ticks
			};

			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
		    {
			    Assert.Throws<FaultException>(() => Target.Handle(setPersonAccountForPersonCommandDto));
		    }
	    }		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<SetPersonAccountForPersonCommandHandler>();
			extend.AddModule(new AssemblerModule());
		}

	}

	[DomainTest]
	public class Bug44502Test : IExtendSystem
	{
		public FakePersonRepository PersonRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public SetPersonAccountForPersonCommandHandler Target;
	
		[Test]
		public void ShouldNotHappen()
		{
			DateTime startDate = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateOnly dateOnly = new DateOnly(startDate);
			DateOnlyDto dateOnlydto1 = new DateOnlyDto { DateTime = startDate.Date.AddDays(-10) };
			DateOnlyDto dateOnlydto2 = new DateOnlyDto { DateTime = startDate.Date };
			DateOnlyDto dateOnlydto3 = new DateOnlyDto { DateTime = startDate.Date.AddDays(10) };

			var scenario = ScenarioRepository.Has("Default");
			var person = PersonFactory.CreatePerson("test").WithId();
			PersonRepository.Add(person);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			absence.Priority = 100;
			absence.InContractTime = true;
			absence.Tracker = Tracker.CreateTimeTracker();
			var absenceSecond = AbsenceFactory.CreateAbsence("test absence").WithId();
			absenceSecond.Priority = 100;
			absenceSecond.InContractTime = true;

			AbsenceRepository.Add(absence);
			AbsenceRepository.Add(absenceSecond);

			var assignment = new PersonAssignment(person, scenario, dateOnly);
			var activity = ActivityFactory.CreateActivity("Phone");
			assignment.AddActivity(activity,new DateTimePeriod(startDate.AddHours(8),startDate.AddHours(17)));
			PersonAssignmentRepository.Add(assignment);

			var assignment2 = new PersonAssignment(person, scenario, dateOnly.AddDays(1));
			assignment2.AddActivity(activity, new DateTimePeriod(startDate.AddDays(1).AddHours(8), startDate.AddDays(1).AddHours(17)));
			PersonAssignmentRepository.Add(assignment2);

			var firstAbsence = new PersonAbsence(person, scenario,
				new AbsenceLayer(absence, new DateTimePeriod(startDate.AddHours(8), startDate.AddHours(17))));
			PersonAbsenceRepository.Add(firstAbsence);

			var firstAbsence2 = new PersonAbsence(person, scenario,
				new AbsenceLayer(absence, new DateTimePeriod(startDate.AddDays(1).AddHours(8), startDate.AddDays(1).AddHours(17))));
			PersonAbsenceRepository.Add(firstAbsence2);

			var secondAbsence = new PersonAbsence(person, scenario,
				new AbsenceLayer(absenceSecond, new DateTimePeriod(startDate.AddHours(8), startDate.AddHours(17))));
			PersonAbsenceRepository.Add(secondAbsence);

			Target.Handle(new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = dateOnlydto1,
				Accrued = 0L,
				BalanceIn = 0L,
				Extra = 0L
			});
			Target.Handle(new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = dateOnlydto2,
				Accrued = TimeSpan.FromHours(10).Ticks,
				BalanceIn = 0L,
				Extra = 0L
			});
			Target.Handle(new SetPersonAccountForPersonCommandDto
			{
				PersonId = person.Id.GetValueOrDefault(),
				AbsenceId = absence.Id.GetValueOrDefault(),
				DateFrom = dateOnlydto3,
				Accrued = 0L,
				BalanceIn = 0L,
				Extra = 0L
			});

			PersonAbsenceAccountRepository.Find(person).Find(absence, dateOnly).LatestCalculatedBalance.Should().Be
				.EqualTo(TimeSpan.FromHours(9));
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<SetPersonAccountForPersonCommandHandler>();
			extend.AddModule(new AssemblerModule());
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Infrastructure.Persisters.Account;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Account
{
	[DatabaseTest]
	public class PersonAccountConflictResolverTest : IExtendSystem, IIsolateSystem
	{
		public IPersonAccountConflictResolver Target;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public IPersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public IPersonRepository PersonRepository;
		public IAbsenceRepository AbsenceRepository;
		public IActivityRepository ActivityRepository;
		public IScenarioRepository ScenarioRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldRecalculate()
		{
			var scenario = new Scenario {DefaultScenario = true};
			var person = new Person().InTimeZone(TimeZoneInfo.Utc);
			var tracker = Tracker.CreateDayTracker();
			var absence = new Absence {Description = new Description("_"), Tracker = tracker};
			var activity = new Activity("_");
			var accountDay = new AccountDay(new DateOnly(2000,1,1));
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Add(accountDay);
			var personAbsence1 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, new DateTimePeriod(2000,1,1,2000,1,2)));
			var ass1 = new PersonAssignment(person, scenario, new DateOnly(2000, 1, 1)).WithLayer(activity, new TimePeriod(8,17));
			var personAbsence2 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, new DateTimePeriod(2001,1,1,2001,1,2)));
			var ass2 = new PersonAssignment(person, scenario, new DateOnly(2001, 1, 1)).WithLayer(activity, new TimePeriod(8,17));
			
			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(absence);
				ActivityRepository.Add(activity);
				PersonAbsenceAccountRepository.Add(personAbsenceAccount);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbsence1);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbsence2);
				PersonAssignmentRepository.Add(ass1);
				PersonAssignmentRepository.Add(ass2);
				setup.PersistAll();
			}

			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnly(2000, 1, 1), person, new List<IPersistableScheduleData> {ass1, ass2, personAbsence1, personAbsence2});

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				Target.Resolve(new []{personAbsenceAccount}, schedulerStateHolder.Schedules);	
			}
			
			//simulate to remove personAbsence1
			accountDay.Track(TimeSpan.Zero);

			personAbsenceAccount.AccountCollection().Single()
				.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromDays(1));
		}
		
		[Test]
		public void ShouldTrackCorrectAccountDay()
		{
			var scenario = new Scenario { DefaultScenario = true };
			var person = new Person().InTimeZone(TimeZoneInfo.Utc);
			var tracker = Tracker.CreateDayTracker();
			var absence = new Absence { Description = new Description("_"), Tracker = tracker };
			var activity = new Activity("_");
			var accountDay1= new AccountDay(new DateOnly(1999,12,31));
			var accountDay2 = new AccountDay(new DateOnly(2000, 1, 1));
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Add(accountDay1);
			personAbsenceAccount.Add(accountDay2);
			var personAbsence1 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, new DateTimePeriod(1999, 12, 31, 2000, 1, 1)));
			var ass1 = new PersonAssignment(person, scenario, new DateOnly(1999, 12, 31)).WithLayer(activity, new TimePeriod(8, 17));
			var personAbsence2 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, new DateTimePeriod(2000, 1, 1, 2000, 1, 2)));
			var ass2 = new PersonAssignment(person, scenario, new DateOnly(2000, 1, 1)).WithLayer(activity, new TimePeriod(8, 17));
			var personAbsence3 = new PersonAbsence(person, scenario, new AbsenceLayer(absence, new DateTimePeriod(2001, 1, 1, 2001, 1, 2)));
			var ass3 = new PersonAssignment(person, scenario, new DateOnly(2001, 1, 1)).WithLayer(activity, new TimePeriod(8, 17));
			using (var setup = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(person);
				AbsenceRepository.Add(absence);
				ActivityRepository.Add(activity);
				PersonAbsenceAccountRepository.Add(personAbsenceAccount);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbsence1);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbsence2);
				((IRepository<IPersonAbsence>)PersonAbsenceRepository).Add(personAbsence3);
				PersonAssignmentRepository.Add(ass1);
				PersonAssignmentRepository.Add(ass2);
				PersonAssignmentRepository.Add(ass3);
				setup.PersistAll();
			}
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(new DateOnly(1999, 12, 31), new DateOnly(2000,1,1)), person, new List<IPersistableScheduleData> { ass1, ass2, ass3, personAbsence1, personAbsence2, personAbsence3 });

			using (CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				Target.Resolve(new[] { personAbsenceAccount }, schedulerStateHolder.Schedules);
			}

			//simulate to remove personAbsence1
			accountDay2.Track(TimeSpan.Zero);

			personAbsenceAccount.AccountCollection()
				.ForEach(account => account.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromDays(1)));
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<PersonAccountConflictResolver>();
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
		}
	}
}
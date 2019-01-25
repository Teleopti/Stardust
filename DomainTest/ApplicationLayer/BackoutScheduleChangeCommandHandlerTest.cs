using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	public class BackoutScheduleChangeCommandHandlerTest : IIsolateSystem, IExtendSystem
	{
		public FakeScheduleHistoryRepository ScheduleHistoryRepository;		
		public FakePersonRepository PersonRepository;
		public FakeLoggedOnUser LoggedOnUser;

		public IScheduleStorage ScheduleStorage;
		public FakeScenarioRepository CurrentScenario;
		public FakeAuditSettingRepository AuditSettingRepository;
		public FakeWriteSideRepository<IAbsence> AbsenceRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public BackoutScheduleChangeCommandHandler Target;
		public FakeEventPublisher EventPublisher;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<BackoutScheduleChangeCommandHandler>();
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<MutableNow>().For<INow, IMutateNow>();
			isolate.UseTestDouble<FakeScheduleHistoryRepository>().For<IScheduleHistoryRepository>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeAggregateRootInitializer>().For<IAggregateRootInitializer>();
			isolate.UseTestDouble<FakeAuditSettingRepository>().For<IAuditSettingRepository>();
			isolate.UseTestDouble<FakeWriteSideRepository<IAbsence>>().For<IProxyForId<IAbsence>>();
			isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void HanlderShouldBeResolved()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldIndicateScheduleAuditTrailNotEnabled()
		{
			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			ScheduleHistoryRepository.ClearRevision();

			AuditSettingRepository.SetSetting(false);

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new [] { new DateOnly(2016,06,11)}
			};

			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(1);
			command.ErrorMessages.First().Should().Be.EqualTo(Resources.ScheduleAuditTrailIsNotRunning);
		}

		[Test]
		public void ShouldIndicateCannotBackoutWhenNoHistoryIsFound()
		{
			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			ScheduleHistoryRepository.ClearRevision();

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new[] { new DateOnly(2016,06,11) }
			};

			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(1);
			command.ErrorMessages.First().Should().Be.EqualTo(Resources.CannotUndoScheduleChange);
		}

		[Test]
		public void ShouldIndicateCannotBackoutWhenLastChangeIsFromDifferentUser()
		{
			var person1 = PersonFactory.CreatePerson("aa", "aa").WithId();
			var person2 = PersonFactory.CreatePerson("bb", "bb").WithId();
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);

			var scenario = CurrentScenario.Has("Default");
			LoggedOnUser.SetFakeLoggedOnUser(person2);
			ScheduleHistoryRepository.ClearRevision();

			var person = PersonFactory.CreatePerson("aa", "aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016, 6, 11, 8, 2016, 6, 11, 17);
			var date = new DateOnly(2016, 6, 11);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, dateTimePeriod);
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision {Id = 2};
			rev.SetRevisionData(person1);
			var lstRev = new Revision {Id = 1};
			lstRev.SetRevisionData(person1);
			ScheduleHistoryRepository.SetRevision(rev, date, pa.CreateTransient());
			pa.SetDayOff(DayOffFactory.CreateDayOff());
			ScheduleHistoryRepository.SetRevision(lstRev, date, pa.CreateTransient());

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new[] {new DateOnly(2016, 06, 11)},
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);

			Target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(1);
			command.ErrorMessages.First().Should().Be.EqualTo(Resources.CannotUndoScheduleChange);
		}

		[Test]
		public void ShouldBackoutToPreviousSchedule()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson("aa", "aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016, 6, 11, 8, 2016, 6, 11, 17);
			var dateOnlyPeriod = new DateOnlyPeriod(2016, 6, 11, 2016, 6, 11);
			var date = new DateOnly(2016, 6, 11);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, dateTimePeriod);
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision {Id = 2};
			rev.SetRevisionData(person);
			var lstRev = new Revision {Id = 1};
			lstRev.SetRevisionData(person);
			ScheduleHistoryRepository.SetRevision(rev, date, pa.CreateTransient());
			pa.SetDayOff(DayOffFactory.CreateDayOff());
			ScheduleHistoryRepository.SetRevision(lstRev, date, pa.CreateTransient());

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new[] {new DateOnly(2016, 06, 11)},
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person, new ScheduleDictionaryLoadOptions(true, true), dateOnlyPeriod, scenario)[person]
				.ScheduledDayCollection(dateOnlyPeriod).Single();

			schedule.SignificantPart().Should().Be.EqualTo(SchedulePartView.DayOff);
		}

		[Test]
		public void BackoutShouldNotAffectAbsenceNotOnTheTargetDate()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016,6,11,8,2016,6,11,17);
			var date = new DateOnly(2016,6,11);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, dateTimePeriod);


			var absence = AbsenceFactory.CreateAbsence("abs").WithId();
			AbsenceRepository.Add(absence);
			var personAbsence = new PersonAbsence(person,scenario,new AbsenceLayer(absence,dateTimePeriod));
			var nextDayPersonAbsence = new PersonAbsence(person, scenario,
				new AbsenceLayer(absence, new DateTimePeriod(2016, 6, 12, 8, 2016, 6, 12, 17)));

			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision { Id = 2 };
			rev.SetRevisionData(person);
			var lstRev = new Revision { Id = 1 };
			lstRev.SetRevisionData(person);

			ScheduleHistoryRepository.SetRevision(rev,date,personAssignment.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev,date,personAbsence.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev,date,personAssignment.CreateTransient());

			ScheduleStorage.Add(nextDayPersonAbsence);
			ScheduleStorage.Add(personAssignment);
		
			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new[] { new DateOnly(2016,06,11) },
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var nextDayAsDateOnlyPeriod = new DateOnlyPeriod(2016, 6, 12, 2016, 6, 12);
			var nextDaySchedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person,new ScheduleDictionaryLoadOptions(true,true),nextDayAsDateOnlyPeriod, scenario)[person]
				.ScheduledDayCollection(nextDayAsDateOnlyPeriod).Single();

			nextDaySchedule.SignificantPart().Should().Be.EqualTo(SchedulePartView.Absence);
		}

		[Test]
		public void ShouldUpdatePersonAccountWhenBackoutSingleDayAbsence()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson("aa", "aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016, 6, 11, 8, 2016, 6, 11, 17);
			var dateOnlyPeriod = new DateOnlyPeriod(2016, 6, 11, 2016, 6, 11);
			var date = new DateOnly(2016, 6, 11);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, dateTimePeriod);

			var absence = AbsenceFactory.CreateAbsence("abs").WithId();
			AbsenceRepository.Add(absence);

			var personAbsence = new PersonAbsence(person, scenario, new AbsenceLayer(absence, dateTimePeriod));
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision {Id = 2};
			rev.SetRevisionData(person);
			var lstRev = new Revision {Id = 1};
			lstRev.SetRevisionData(person);

			ScheduleHistoryRepository.SetRevision(rev, date, personAssignment.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev, date, personAbsence.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev, date, personAssignment.CreateTransient());


			var accountDay = createAccountDay(new DateOnly(2016, 6, 1), TimeSpan.FromDays(0), TimeSpan.FromDays(3),
				TimeSpan.FromDays(0));

			var account = PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person, absence, accountDay);
			PersonAbsenceAccountRepository.Add(account);

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new[] {new DateOnly(2016, 06, 11)},
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person, new ScheduleDictionaryLoadOptions(true, true), dateOnlyPeriod, scenario)[person]
				.ScheduledDayCollection(dateOnlyPeriod).Single();

			schedule.SignificantPart().Should().Be.EqualTo(SchedulePartView.FullDayAbsence);
			accountDay.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromDays(1));
		}

		[Test]
		public void ShouldBackoutEvenPersonAccountIsExceeded()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016,6,11,8,2016,6,11,17);
			var dateOnlyPeriod = new DateOnlyPeriod(2016,6,11,2016,6,11);
			var date = new DateOnly(2016,6,11);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, dateTimePeriod);

			var absence = AbsenceFactory.CreateAbsence("abs").WithId();
			AbsenceRepository.Add(absence);

			var personAbsence = new PersonAbsence(person,scenario,new AbsenceLayer(absence,dateTimePeriod));
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision { Id = 2 };
			rev.SetRevisionData(person);
			var lstRev = new Revision { Id = 1 };
			lstRev.SetRevisionData(person);

			ScheduleHistoryRepository.SetRevision(rev,date,personAssignment.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev,date,personAbsence.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev,date,personAssignment.CreateTransient());


			var accountDay = createAccountDay(new DateOnly(2016,6,1),TimeSpan.FromDays(0),TimeSpan.FromDays(0),
				TimeSpan.FromDays(0));

			var account = PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person,absence,accountDay);
			PersonAbsenceAccountRepository.Add(account);

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new[] { new DateOnly(2016,06,11) },
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person,new ScheduleDictionaryLoadOptions(true,true),dateOnlyPeriod,scenario)[person]
				.ScheduledDayCollection(dateOnlyPeriod).Single();

			schedule.SignificantPart().Should().Be.EqualTo(SchedulePartView.FullDayAbsence);
			accountDay.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromDays(1));
		}

		[Test]
		public void ShouldUpdatePersonAccountWhenBackoutMultiDayAbsence()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			
			var multiDayDateTimePeriod = new DateTimePeriod(2016, 6, 11, 0, 2016, 6, 13, 0);			

			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, new DateTimePeriod(2016,6,11,8,2016,6,11,17));
			var nextDayPersonAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,scenario, new DateTimePeriod(2016,6,12,8,2016,6,12,17));
			
			var absence = AbsenceFactory.CreateAbsence("abs").WithId();
			AbsenceRepository.Add(absence);

			var personAbsence = new PersonAbsence(person,scenario,new AbsenceLayer(absence,multiDayDateTimePeriod));
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision { Id = 2 };
			rev.SetRevisionData(person);
			var lstRev = new Revision { Id = 1 };
			lstRev.SetRevisionData(person);

			ScheduleHistoryRepository.SetRevision(rev, new DateOnly(2016, 6, 11), personAssignment.CreateTransient());
			ScheduleHistoryRepository.SetRevision(rev, new DateOnly(2016,6,12),nextDayPersonAssignment.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev,new DateOnly(2016,6,11),personAbsence.CreateTransient());			
			ScheduleHistoryRepository.SetRevision(lstRev,new DateOnly(2016,6,11),personAssignment.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev,new DateOnly(2016,6,12),personAbsence.CreateTransient());
			ScheduleHistoryRepository.SetRevision(lstRev,new DateOnly(2016,6,12),nextDayPersonAssignment.CreateTransient());

			ScheduleStorage.Add(nextDayPersonAssignment);
			ScheduleStorage.Add( personAssignment);

			var accountDay = createAccountDay(new DateOnly(2016,6,1),TimeSpan.FromDays(0),TimeSpan.FromDays(3),
				TimeSpan.FromDays(0));

			var account = PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person,absence,accountDay);
			PersonAbsenceAccountRepository.Add(account);

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new[] { new DateOnly(2016,06,11) },
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var schedules = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person,new ScheduleDictionaryLoadOptions(true,true), new DateOnlyPeriod(2016, 6, 11, 2016, 6, 12), scenario)[person];

			var directlyAffectedSchedule = schedules.ScheduledDay(new DateOnly(2016, 6, 11));
			var indirectlyAffectedSchedule = schedules.ScheduledDay(new DateOnly(2016, 6, 12));

			directlyAffectedSchedule.SignificantPart().Should().Be.EqualTo(SchedulePartView.FullDayAbsence);
			indirectlyAffectedSchedule.SignificantPart().Should().Be.EqualTo(SchedulePartView.FullDayAbsence);
			accountDay.LatestCalculatedBalance.Should().Be.EqualTo(TimeSpan.FromDays(2));
		}

		[Test]
		public void ShouldRaiseEventWithCorrectPeriod()
		{
			var scenario = CurrentScenario.Has("Default");
			var person = PersonFactory.CreatePerson("aa", "aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016, 6, 11, 8, 2016, 6, 11, 17);
			var date = new DateOnly(2016, 6, 11);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, dateTimePeriod);
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision {Id = 2};
			rev.SetRevisionData(person);
			var lstRev = new Revision {Id = 1};
			lstRev.SetRevisionData(person);
			ScheduleHistoryRepository.SetRevision(rev, date, pa.CreateTransient());
			pa.SetDayOff(DayOffFactory.CreateDayOff());
			ScheduleHistoryRepository.SetRevision(lstRev, date, pa.CreateTransient());
			pa.PopAllEvents(null);

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Dates = new[] {new DateOnly(2016, 06, 11)},
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			Target.Handle(command);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(1);
			var myEvent =
			EventPublisher.PublishedEvents.First() as ScheduleBackoutEvent;
			myEvent.StartDateTime.Should().Be.EqualTo(pa.Period.StartDateTime);
			myEvent.EndDateTime.Should().Be.EqualTo(pa.Period.EndDateTime);
			myEvent.CommandId.Should().Be.EqualTo(command.TrackedCommandInfo.TrackId);
			myEvent.PersonId.Should().Be.EqualTo(person.Id);
		}


		private AccountDay createAccountDay(DateOnly startDate,TimeSpan balanceIn,TimeSpan accrued,TimeSpan balance)
		{
			return new AccountDay(startDate)
			{
				BalanceIn = balanceIn,
				Accrued = accrued,
				Extra = TimeSpan.FromDays(0),
				LatestCalculatedBalance = balance
			};
		}

	}	

	public class FakeAuditSettingRepository: IAuditSettingRepository
	{
		private bool _setting = true;

		public void TruncateAndMoveScheduleFromCurrentToAuditTables()
		{
			throw new NotImplementedException();
		}

		public void SetSetting(bool setting)
		{
			_setting = setting;
		}

		public IAuditSetting Read()
		{
			var setting = new FakeAuditSetting();
			setting.SetScheduleEnabled(_setting);
			return setting;
		}

		internal class FakeAuditSetting : IAuditSetting
		{
			public void SetScheduleEnabled(bool v)
			{
				IsScheduleEnabled = v;
			}

			public bool IsScheduleEnabled { get; protected set; }
			public void TurnOffScheduleAuditing(IAuditSetter auditSettingSetter)
			{
				throw new NotImplementedException();
			}

			public void TurnOnScheduleAuditing(IAuditSettingRepository auditSettingRepository, IAuditSetter auditSettingSetter)
			{
				throw new NotImplementedException();
			}

			public bool ShouldBeAudited(object entity)
			{
				throw new NotImplementedException();
			}
		}
	}
}

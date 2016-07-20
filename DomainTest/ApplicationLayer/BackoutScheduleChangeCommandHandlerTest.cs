using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[DomainTest]
	[TestFixture]
	public class BackoutScheduleChangeCommandHandlerTest : ISetup
	{
		public FakeScheduleHistoryRepository ScheduleHistoryRepository;		
		public FakePersonRepository PersonRepository;
		public FakeLoggedOnUser LoggedOnUser;

		public FakeScheduleStorage ScheduleStorage;
		public FakeCurrentScenario CurrentScenario;
		public FakeScheduleDifferenceSaver ScheduleDifferenceSaver;
		public FakeAuditSettingRepository AuditSettingRepository;
		public BackoutScheduleChangeCommandHandler target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleHistoryRepository>().For<IScheduleHistoryRepository>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeScheduleDifferenceSaver>().For<IScheduleDifferenceSaver>();
			system.UseTestDouble<FakeAggregateRootInitializer>().For<IAggregateRootInitializer>();
			system.UseTestDouble<FakeAuditSettingRepository>().For<IAuditSettingRepository>();
			system.AddService<BackoutScheduleChangeCommandHandler>();
		}

		[Test]
		public void HanlderShouldBeResolved()
		{
			target.Should().Not.Be.Null();
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
				Date = new DateOnly(2016,06,11)
			};

			target.Handle(command);

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
				Date = new DateOnly(2016, 06, 11)
			};

			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(1);
			command.ErrorMessages.First().Should().Be.EqualTo(Resources.CannotUndoScheduleChange);
		}

		[Test]
		public void ShouldIndicateCannotBackoutWhenLastChangeIsFromDifferentUser()
		{
			var person1 = PersonFactory.CreatePerson("aa","aa").WithId();
			var person2 = PersonFactory.CreatePerson("bb","bb").WithId();
			PersonRepository.Add(person1);
			PersonRepository.Add(person2);

			LoggedOnUser.SetFakeLoggedOnUser(person2);
			ScheduleHistoryRepository.ClearRevision();

			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016,6,11,8,2016,6,11,17);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(CurrentScenario.Current(),person,dateTimePeriod);
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision { Id = 2 };
			rev.SetRevisionData(person1);
			var lstRev = new Revision { Id = 1 };
			lstRev.SetRevisionData(person1);
			ScheduleHistoryRepository.SetRevision(rev,pa.CreateTransient());
			pa.SetDayOff(DayOffFactory.CreateDayOff());
			ScheduleHistoryRepository.SetRevision(lstRev,pa.CreateTransient());

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Date = new DateOnly(2016,06,11),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			target.Handle(command);
		
			target.Handle(command);

			command.ErrorMessages.Count.Should().Be.EqualTo(1);
			command.ErrorMessages.First().Should().Be.EqualTo(Resources.CannotUndoScheduleChange);
		}

		[Test]
		public void ShouldBackoutToPreviousSchedule()
		{
			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016, 6, 11, 8, 2016, 6, 11, 17);
			var dateOnlyPeriod = new DateOnlyPeriod(2016, 6, 11, 2016, 6, 11);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(CurrentScenario.Current(), person, dateTimePeriod);
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision { Id = 2 };
			rev.SetRevisionData(person);
			var lstRev = new Revision { Id = 1};
			lstRev.SetRevisionData(person);
			ScheduleHistoryRepository.SetRevision(rev, pa.CreateTransient());
			pa.SetDayOff(DayOffFactory.CreateDayOff());
			ScheduleHistoryRepository.SetRevision(lstRev, pa.CreateTransient());

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Date = new DateOnly(2016,06,11),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			target.Handle(command);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person, new ScheduleDictionaryLoadOptions(true, true), dateOnlyPeriod, CurrentScenario.Current())[person].ScheduledDayCollection(dateOnlyPeriod).Single();

			schedule.SignificantPart().Should().Be.EqualTo(SchedulePartView.DayOff);
					}

		[Test]
		public void ShouldAssignCommandIdToAllEvents()
		{
			var person = PersonFactory.CreatePerson("aa","aa").WithId();
			PersonRepository.Add(person);
			LoggedOnUser.SetFakeLoggedOnUser(person);
			var dateTimePeriod = new DateTimePeriod(2016,6,11,8,2016,6,11,17);
			var dateOnlyPeriod = new DateOnlyPeriod(2016,6,11,2016,6,11);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(CurrentScenario.Current(),person,dateTimePeriod);
			ScheduleHistoryRepository.ClearRevision();
			var rev = new Revision { Id = 2 };
			rev.SetRevisionData(person);
			var lstRev = new Revision { Id = 1 };
			lstRev.SetRevisionData(person);
			ScheduleHistoryRepository.SetRevision(rev,pa.CreateTransient());
			pa.SetDayOff(DayOffFactory.CreateDayOff());
			ScheduleHistoryRepository.SetRevision(lstRev,pa.CreateTransient());

			var command = new BackoutScheduleChangeCommand
			{
				PersonId = person.Id.Value,
				Date = new DateOnly(2016,06,11),
				TrackedCommandInfo = new TrackedCommandInfo
				{
					TrackId = Guid.NewGuid()
				}
			};
			target.Handle(command);
			command.ErrorMessages.Count.Should().Be.EqualTo(0);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
				person,new ScheduleDictionaryLoadOptions(true,true),dateOnlyPeriod,CurrentScenario.Current())[person].ScheduledDayCollection(dateOnlyPeriod).Single();


			var events = schedule.PersonAssignment().PopAllEvents();

			foreach (var e in events)
			{
				var _e = e as ICommandIdentifier;
				_e.Should().Not.Be.Null();
				_e.CommandId.Should().Be.EqualTo(command.TrackedCommandInfo.TrackId);
			}			
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

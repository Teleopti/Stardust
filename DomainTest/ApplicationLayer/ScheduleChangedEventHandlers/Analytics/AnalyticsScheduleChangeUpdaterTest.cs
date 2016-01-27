using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsScheduleChangeUpdaterTest
	{
		private AnalyticsScheduleChangeUpdater _target;
		private IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private IAnalyticsFactScheduleDateHandler _dateHandler;
		private IAnalyticsFactSchedulePersonHandler _personHandler;
		private IAnalyticsFactScheduleDayCountHandler _scheduleDayCountHandler;
		private IAnalyticsFactScheduleHandler _factScheduleHandler;
		private IDelayedMessageSender _sendDelayedMessages;

		[SetUp]
		public void Setup()
		{
			_factScheduleHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleHandler>();
			_dateHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDateHandler>();
			_personHandler = MockRepository.GenerateMock<IAnalyticsFactSchedulePersonHandler>();
			_scheduleDayCountHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDayCountHandler>();
			_analyticsScheduleRepository = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			_sendDelayedMessages = MockRepository.GenerateMock<IDelayedMessageSender>();
			_target = new AnalyticsScheduleChangeUpdater(
				_factScheduleHandler,
				_dateHandler,
				_personHandler,
				_scheduleDayCountHandler,
				_analyticsScheduleRepository,
				_sendDelayedMessages);
		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenDateIdMappingFails()
		{
			var scenario = new AnalyticsGeneric { Code = Guid.NewGuid(), Id = 6 };
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = DateTime.Now,
				PersonPeriodId = Guid.NewGuid()
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.Code,
			};
			var personPart = new AnalyticsFactSchedulePerson();

			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new IAnalyticsGeneric[] { scenario });
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric>());
			const int dateId = 0;
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(false);

			_target.Handle(@event);
			
			_analyticsScheduleRepository.AssertWasNotCalled(x => x.DeleteFactSchedule(dateId, personPart.PersonId, scenario.Id));
		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenPersonPeriodIdIsEmpty()
		{
			var scenario = new AnalyticsGeneric { Code = Guid.NewGuid(), Id = 6 };
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = DateTime.Now,
				PersonPeriodId = Guid.Empty
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.Code,
			};
			var personPart = new AnalyticsFactSchedulePerson();

			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new IAnalyticsGeneric[] { scenario });
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric>());
			const int dateId = 0;
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasNotCalled(x => x.DeleteFactSchedule(dateId, personPart.PersonId, scenario.Id));
		}

		[Test]
		public void ShouldSaveFactScheduleAndFactScheduleDayCount()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var shift = new ProjectionChangedEventShift
			{
				StartDateTime = start,
				EndDateTime = start.AddHours(2).AddMinutes(15)
			};
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = shift,
				ShiftCategoryId = Guid.NewGuid(),
				PersonPeriodId = Guid.NewGuid()
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = Guid.NewGuid()
			};
			var cat = new AnalyticsGeneric { Id = 55, Code = scheduleDay.ShiftCategoryId };
			var scenario = new AnalyticsGeneric { Id = 66, Code = @event.ScenarioId };
			var scheduleRow = new FactScheduleRow
			{
				DatePart = new AnalyticsFactScheduleDate(),
				PersonPart = new AnalyticsFactSchedulePerson{PersonId = 11},
				TimePart = new AnalyticsFactScheduleTime()
			};
			var factScheduleRows = new List<IFactScheduleRow>{ scheduleRow };
			var scheduleDayCountPart = new AnalyticsFactScheduleDayCount();
			const int dateId = 0;

			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric> { cat });
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(scheduleRow.PersonPart);
			_factScheduleHandler.Stub(
				x =>
					x.AgentDaySchedule(Arg<ProjectionChangedEventScheduleDay>.Is.Anything, Arg<IAnalyticsFactSchedulePerson>.Is.Anything, 
						Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything))
						.Return(factScheduleRows);
			_scheduleDayCountHandler.Stub(
				x =>
					x.Handle(Arg<ProjectionChangedEventScheduleDay>.Is.Anything, Arg<IAnalyticsFactSchedulePerson>.Is.Anything,
						Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(scheduleDayCountPart);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleBatch(factScheduleRows), y => y.Repeat.Times(1));
			_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleDayCountRow(scheduleDayCountPart), y => y.Repeat.Times(1));
		}

		[Test]
		public void ShouldOnlyDeleteScheduleIfNotScheduled()
		{
			var scenario = new AnalyticsGeneric { Code = Guid.NewGuid(), Id = 6 };
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = new DateTime(2014, 12, 03),
				NotScheduled = true
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.Code
			};
			const int dateId = -1;
			var personPart = new AnalyticsFactSchedulePerson {PersonId = 55};
			_personHandler.Stub(x => x.Handle(Arg<Guid>.Is.Anything)).Return(personPart);
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric>());
			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });
			_dateHandler.Stub(
				x => x.MapDateId(Arg.Is(new DateOnly(scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);
			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.DeleteFactSchedule(dateId, 55, scenario.Id));
			_analyticsScheduleRepository.AssertWasNotCalled(x => x.PersistFactScheduleBatch(Arg<IList<IFactScheduleRow>>.Is.Anything));
		}

		[Test]
		public void ShouldDeleteShiftIfLayerStartDateFailToMap()
		{
			var start = new DateTime(2014, 12, 01, 23, 30, 0, DateTimeKind.Utc);
			var shift = new ProjectionChangedEventShift
			{
				StartDateTime = start,
				EndDateTime = start.AddHours(1)
			};
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = shift,
				ShiftCategoryId = Guid.NewGuid(),
				PersonPeriodId = Guid.NewGuid()
			};

			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = Guid.NewGuid()
			};
			var cat = new AnalyticsGeneric { Id = 55, Code = scheduleDay.ShiftCategoryId };
			var scenario = new AnalyticsGeneric { Id = 66, Code = @event.ScenarioId };
			var personPart = new AnalyticsFactSchedulePerson{ PersonId = 2};
			var scheduleDayCountPart = new AnalyticsFactScheduleDayCount();
			const int dateId = 0;

			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric> { cat });
			_factScheduleHandler.Stub(
				x =>
					x.AgentDaySchedule(Arg<ProjectionChangedEventScheduleDay>.Is.Anything, Arg<IAnalyticsFactSchedulePerson>.Is.Anything,
						Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything))
						.Return(null);
			_scheduleDayCountHandler.Stub(
				x =>
					x.Handle(Arg<ProjectionChangedEventScheduleDay>.Is.Anything, Arg<IAnalyticsFactSchedulePerson>.Is.Anything,
						Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(scheduleDayCountPart);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.DeleteFactSchedule(dateId, personPart.PersonId, scenario.Id), y => y.Repeat.Times(2));
			_analyticsScheduleRepository.AssertWasNotCalled(x => x.PersistFactScheduleBatch(Arg<IList<IFactScheduleRow>>.Is.Anything));
			_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleDayCountRow(scheduleDayCountPart), y => y.Repeat.Times(1));
		}

		[Test]
		public void ShouldPersistScheduleDayCountWhenDayOff()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				DayOff = new ProjectionChangedEventDayOff(),
				PersonPeriodId = Guid.NewGuid()
			};
			var scenario = new AnalyticsGeneric { Code = Guid.NewGuid(), Id = 6 };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.Code
			};
			var dayCount = new AnalyticsFactScheduleDayCount();
			var personPart = new AnalyticsFactSchedulePerson{PersonId = 11};
			const int dateId = 0;
			
			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });
			
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_scheduleDayCountHandler.Stub(x => x.Handle(scheduleDay, personPart, scenario.Id, -1)).Return(dayCount);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleDayCountRow(dayCount));
		}

		[Test]
		public void ShouldBeTolerantAndNotTryToPersistIfDayCountIsNull()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				DayOff = new ProjectionChangedEventDayOff(),
				PersonPeriodId = Guid.NewGuid()
			};
			var scenario = new AnalyticsGeneric { Code = Guid.NewGuid(), Id = 6 };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.Code
			};

			var personPart = new AnalyticsFactSchedulePerson();
			const int dateId = 0;

			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });

			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_scheduleDayCountHandler.Stub(x => x.Handle(scheduleDay, personPart, scenario.Id, -1)).Return(null);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasNotCalled(x => x.PersistFactScheduleDayCountRow(null));
		}

		[Test]
		public void ShouldResendMessageOnTimeout()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				DayOff = new ProjectionChangedEventDayOff(),
				PersonPeriodId = Guid.NewGuid(),
				Date = new DateTime(2015,8,25)
			};
			var scenario = new AnalyticsGeneric { Code = Guid.NewGuid(), Id = 6 };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.Code
			};
			var personPart = new AnalyticsFactSchedulePerson { PersonId = 11 };
			const int dateId = 0;

			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });

			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);

			var sqlException = SqlExceptionConstructor.CreateSqlException("Timeout", -2);

			_analyticsScheduleRepository.Stub(a => a.DeleteFactSchedule(dateId, 11,6)).Throw(sqlException);

			_target.Handle(@event);

			_sendDelayedMessages.AssertWasCalled(x => x.DelaySend(Arg<DateTime>.Is.Anything, Arg<ProjectionChangedEventScheduleDay>.Is.Anything));
		}

		[Test]
		public void ShouldOnlyResendFiveTimesOnTimeout()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				DayOff = new ProjectionChangedEventDayOff(),
				PersonPeriodId = Guid.NewGuid(),
				Date = new DateTime(2015, 8, 25)
			};
			var scenario = new AnalyticsGeneric { Code = Guid.NewGuid(), Id = 6 };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.Code,
				RetriesCount = 5
			};
			var personPart = new AnalyticsFactSchedulePerson { PersonId = 11 };
			const int dateId = 0;

			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });

			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);

			var sqlException = SqlExceptionConstructor.CreateSqlException("Timeout", -2);

			_analyticsScheduleRepository.Stub(a => a.DeleteFactSchedule(dateId, 11, 6)).Throw(sqlException);

			_target.Handle(@event);

			_sendDelayedMessages.AssertWasNotCalled(x => x.DelaySend(Arg<DateTime>.Is.Anything, Arg<ProjectionChangedEventScheduleDay>.Is.Anything));
		}
	}
}
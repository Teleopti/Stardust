using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
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
		private IAnalyticsScheduleChangeUpdaterFilter _analyticsScheduleChangeUpdaterFilter;
		private IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private IAnalyticsShiftCategoryRepository _analyticsShiftCategoryRepository;

		[SetUp]
		public void Setup()
		{
			_factScheduleHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleHandler>();
			_dateHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDateHandler>();
			_personHandler = MockRepository.GenerateMock<IAnalyticsFactSchedulePersonHandler>();
			_scheduleDayCountHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDayCountHandler>();
			_analyticsScheduleRepository = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			_sendDelayedMessages = MockRepository.GenerateMock<IDelayedMessageSender>();
			_analyticsScheduleChangeUpdaterFilter = MockRepository.GenerateMock<IAnalyticsScheduleChangeUpdaterFilter>();
			_analyticsScheduleChangeUpdaterFilter.Stub(x => x.ContinueProcessingEvent(Arg<ProjectionChangedEvent>.Is.Anything))
				.Return(true);
			_analyticsScenarioRepository = new FakeAnalyticsScenarioRepository();
			_analyticsShiftCategoryRepository = MockRepository.GenerateMock<IAnalyticsShiftCategoryRepository>();

			_target = new AnalyticsScheduleChangeUpdater(
				_factScheduleHandler,
				_dateHandler,
				_personHandler,
				_scheduleDayCountHandler,
				_analyticsScheduleRepository,
				_sendDelayedMessages,
				_analyticsScheduleChangeUpdaterFilter,
				_analyticsScenarioRepository,
				_analyticsShiftCategoryRepository);
		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenDateIdMappingFails()
		{
			var scenario = new AnalyticsScenario { ScenarioCode = Guid.NewGuid() };
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = DateTime.Now,
				PersonPeriodId = Guid.NewGuid()
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.ScenarioCode.GetValueOrDefault()
			};
			var personPart = new AnalyticsFactSchedulePerson();

			_analyticsScenarioRepository.AddScenario(scenario);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_analyticsShiftCategoryRepository.Stub(x => x.ShiftCategories()).Return(new List<AnalyticsShiftCategory>());
			const int dateId = 0;
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(false);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasNotCalled(x => x.DeleteFactSchedule(dateId, personPart.PersonId, scenario.ScenarioId));
		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenPersonPeriodIdIsEmpty()
		{
			var scenario = new AnalyticsScenario { ScenarioCode = Guid.NewGuid(), ScenarioId = 6 };
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = DateTime.Now,
				PersonPeriodId = Guid.Empty
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.ScenarioCode.GetValueOrDefault(),
			};
			var personPart = new AnalyticsFactSchedulePerson();

			_analyticsScenarioRepository.AddScenario(scenario);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_analyticsShiftCategoryRepository.Stub(x => x.ShiftCategories()).Return(new List<AnalyticsShiftCategory>());
			const int dateId = 0;
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasNotCalled(x => x.DeleteFactSchedule(dateId, personPart.PersonId, scenario.ScenarioId));
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
			var cat = new AnalyticsShiftCategory { ShiftCategoryId = 55, ShiftCategoryCode = scheduleDay.ShiftCategoryId };
			var scenario = new AnalyticsScenario { ScenarioCode = @event.ScenarioId, ScenarioId = 66 };
			var scheduleRow = new FactScheduleRow
			{
				DatePart = new AnalyticsFactScheduleDate(),
				PersonPart = new AnalyticsFactSchedulePerson{PersonId = 11},
				TimePart = new AnalyticsFactScheduleTime()
			};
			var factScheduleRows = new List<IFactScheduleRow>{ scheduleRow };
			var scheduleDayCountPart = new AnalyticsFactScheduleDayCount();
			const int dateId = 0;

			_analyticsScenarioRepository.AddScenario(scenario);
			_analyticsShiftCategoryRepository.Stub(x => x.ShiftCategories()).Return(new List<AnalyticsShiftCategory> { cat });
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
			var scenario = new AnalyticsScenario { ScenarioCode = Guid.NewGuid(), ScenarioId = 6 };
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = new DateTime(2014, 12, 03),
				NotScheduled = true
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.ScenarioCode.GetValueOrDefault()
			};
			const int dateId = -1;
			var personPart = new AnalyticsFactSchedulePerson {PersonId = 55};
			_personHandler.Stub(x => x.Handle(Arg<Guid>.Is.Anything)).Return(personPart);
			_analyticsShiftCategoryRepository.Stub(x => x.ShiftCategories()).Return(new List<AnalyticsShiftCategory>());
			_analyticsScenarioRepository.AddScenario(scenario);
			_dateHandler.Stub(
				x => x.MapDateId(Arg.Is(new DateOnly(scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);
			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.DeleteFactSchedule(dateId, 55, scenario.ScenarioId));
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
			var cat = new AnalyticsShiftCategory { ShiftCategoryId = 55, ShiftCategoryCode = scheduleDay.ShiftCategoryId };
			var scenario = new AnalyticsScenario { ScenarioCode = @event.ScenarioId, ScenarioId = 66 };
			var personPart = new AnalyticsFactSchedulePerson{ PersonId = 2};
			var scheduleDayCountPart = new AnalyticsFactScheduleDayCount();
			const int dateId = 0;

			_analyticsScenarioRepository.AddScenario(scenario);
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_analyticsShiftCategoryRepository.Stub(x => x.ShiftCategories()).Return(new List<AnalyticsShiftCategory> { cat });
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

			_analyticsScheduleRepository.AssertWasCalled(x => x.DeleteFactSchedule(dateId, personPart.PersonId, scenario.ScenarioId), y => y.Repeat.Times(2));
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
			var scenario = new AnalyticsScenario { ScenarioCode = Guid.NewGuid(), ScenarioId = 6 };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.ScenarioCode.GetValueOrDefault()
			};
			var dayCount = new AnalyticsFactScheduleDayCount();
			var personPart = new AnalyticsFactSchedulePerson{PersonId = 11};
			const int dateId = 0;

			_analyticsScenarioRepository.AddScenario(scenario);

			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_scheduleDayCountHandler.Stub(x => x.Handle(scheduleDay, personPart, scenario.ScenarioId, -1)).Return(dayCount);

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
			var scenario = new AnalyticsScenario { ScenarioCode = Guid.NewGuid(), ScenarioId = 6 };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.ScenarioCode.GetValueOrDefault()
			};

			var personPart = new AnalyticsFactSchedulePerson();
			const int dateId = 0;

			_analyticsScenarioRepository.AddScenario(scenario);

			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_scheduleDayCountHandler.Stub(x => x.Handle(scheduleDay, personPart, scenario.ScenarioId, -1)).Return(null);

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
			var scenario = new AnalyticsScenario { ScenarioCode = Guid.NewGuid(), ScenarioId = 6 };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.ScenarioCode.GetValueOrDefault()
			};
			var personPart = new AnalyticsFactSchedulePerson { PersonId = 11 };
			const int dateId = 0;

			_analyticsScenarioRepository.AddScenario(scenario);

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
			var scenario = new AnalyticsScenario { ScenarioCode = Guid.NewGuid(), ScenarioId = 6 };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = scenario.ScenarioCode.GetValueOrDefault(),
				RetriesCount = 5
			};
			var personPart = new AnalyticsFactSchedulePerson { PersonId = 11 };
			const int dateId = 0;

			_analyticsScenarioRepository.AddScenario(scenario);

			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);

			var sqlException = SqlExceptionConstructor.CreateSqlException("Timeout", -2);

			_analyticsScheduleRepository.Stub(a => a.DeleteFactSchedule(dateId, 11, 6)).Throw(sqlException);

			_target.Handle(@event);

			_sendDelayedMessages.AssertWasNotCalled(x => x.DelaySend(Arg<DateTime>.Is.Anything, Arg<ProjectionChangedEventScheduleDay>.Is.Anything));
		}

		[Test]
		public void ShouldNotBeHandledScenarioDoesNotExistInAnalytics()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				DayOff = new ProjectionChangedEventDayOff(),
				PersonPeriodId = Guid.NewGuid()
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = Guid.NewGuid()
			};

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasNotCalled(x => x.DeleteFactSchedule(Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything));
			_analyticsScheduleRepository.AssertWasNotCalled(x => x.PersistFactScheduleBatch(Arg<IList<IFactScheduleRow>>.Is.Anything));
		}

		[Test]
		public void ShouldNotBeHandledBecauseFilter()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				DayOff = new ProjectionChangedEventDayOff(),
				PersonPeriodId = Guid.NewGuid()
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay },
				ScenarioId = Guid.NewGuid()
			};

			_analyticsScheduleChangeUpdaterFilter.Stub(x => x.ContinueProcessingEvent(Arg<ProjectionChangedEvent>.Is.Anything))
				.Return(false).Repeat.Any();

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasNotCalled(x => x.DeleteFactSchedule(Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything));
			_analyticsScheduleRepository.AssertWasNotCalled(x => x.PersistFactScheduleBatch(Arg<IList<IFactScheduleRow>>.Is.Anything));
		}
	}
}
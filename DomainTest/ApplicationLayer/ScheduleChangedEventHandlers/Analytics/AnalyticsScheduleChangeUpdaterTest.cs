using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsScheduleChangeUpdaterTest
	{
		private IIntervalLengthFetcher _intervalLengthFetcher;
		private AnalyticsScheduleChangeUpdater _target;
		private IAnalyticsFactScheduleTimeHandler _timeHandler;
		private IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private IAnalyticsFactScheduleDateHandler _dateHandler;
		private IAnalyticsFactSchedulePersonHandler _personHandler;
		private IAnalyticsFactScheduleDayCountHandler _scheduleDayCountHandler;

		[SetUp]
		public void Setup()
		{
			_intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			_timeHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleTimeHandler>();
			_dateHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDateHandler>();
			_personHandler = MockRepository.GenerateMock<IAnalyticsFactSchedulePersonHandler>();
			_scheduleDayCountHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDayCountHandler>();
			_analyticsScheduleRepository = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			_target = new AnalyticsScheduleChangeUpdater(
				_intervalLengthFetcher,
				_timeHandler,
				_dateHandler,
				_personHandler,
				_scheduleDayCountHandler,
				_analyticsScheduleRepository);
		}

		[Test]
		public void ShouldKnowIntervalLength()
		{
			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric>());
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric>());
			_target.Handle(new ProjectionChangedEvent { ScheduleDays = new Collection<ProjectionChangedEventScheduleDay>() });
			_intervalLengthFetcher.AssertWasCalled(x => x.IntervalLength);

		}

		[Test]
		public void ShouldNotTryToSaveScheduleChangeWhenDateIdMappingFails()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = DateTime.Now,
				PersonPeriodId = Guid.NewGuid()
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay }
			};

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			var personPart = new AnalyticsFactSchedulePerson();
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric>());
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric>());
			const int dateId = 0;
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(false);

			_target.Handle(@event);
			
			_analyticsScheduleRepository.AssertWasNotCalled(x => x.DeleteFactSchedule(dateId, personPart.PersonId));
		}

		[Test]
		public void ShouldGetLayerAndSendToRepository()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start, new[] { 60, 15, 60 });
			var shift = new ProjectionChangedEventShift
			{
				StartDateTime = start,
				EndDateTime = start.AddHours(2).AddMinutes(15),
				Layers = list
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
			var timePart = new AnalyticsFactScheduleTime();
			var datePart = new AnalyticsFactScheduleDate();
			var personPart = new AnalyticsFactSchedulePerson();
			var scheduleDayCountPart = new AnalyticsFactScheduleDayCount();
			const int dateId = 0;

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric> { cat });
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_timeHandler.Stub(
				x => x.Handle(Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<int>.Is.Equal(55), Arg<int>.Is.Equal(66)))
				.Return(timePart);
			_dateHandler.Stub(
				x =>
					x.Handle(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<DateOnly>.Is.Anything,
						Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything)).Return(datePart);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_scheduleDayCountHandler.Stub(
				x =>
					x.Handle(Arg<ProjectionChangedEventScheduleDay>.Is.Anything, Arg<IAnalyticsFactSchedulePerson>.Is.Anything,
						Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(scheduleDayCountPart);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleRow(timePart, datePart, personPart), y => y.Repeat.Times(9));
			_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleDayCountRow(scheduleDayCountPart), y => y.Repeat.Times(1));
		}

		[Test]
		public void ShouldOnlyDeleteScheduleIfNotScheduled()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Date = new DateTime(2014, 12, 03),
				NotScheduled = true
			};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay }
			};
			var timePart = new AnalyticsFactScheduleTime();
			const int dateId = -1;
			var personPart = new AnalyticsFactSchedulePerson {PersonId = 55};
			_personHandler.Stub(x => x.Handle(Arg<Guid>.Is.Anything)).Return(personPart);
			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric>());
			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric>());
			_dateHandler.Stub(
				x => x.MapDateId(Arg.Is(new DateOnly(scheduleDay.Date)), out Arg<int>.Out(dateId).Dummy)).Return(true);
			_timeHandler.Stub(
				x => x.Handle(Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything))
				.Return(timePart);
			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.DeleteFactSchedule(dateId, 55));
			_analyticsScheduleRepository.AssertWasNotCalled(
				x =>
					x.PersistFactScheduleRow(Arg<AnalyticsFactScheduleTime>.Is.Anything, Arg<AnalyticsFactScheduleDate>.Is.Anything,
						Arg<AnalyticsFactSchedulePerson>.Is.Anything));
		}

		[Test]
		public void ShouldDeleteShiftIfLayerStartDateFailToMap()
		{
			var start = new DateTime(2014, 12, 01, 23, 30, 0, DateTimeKind.Utc);
			var list = createLayers(start, new[] { 60 });
			var shift = new ProjectionChangedEventShift
			{
				StartDateTime = start,
				EndDateTime = start.AddHours(1),
				Layers = list
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

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			_analyticsScheduleRepository.Stub(x => x.Scenarios()).Return(new List<IAnalyticsGeneric> { scenario });
			_dateHandler.Stub(x => x.MapDateId(Arg<DateOnly>.Is.Anything, out Arg<int>.Out(dateId).Dummy)).Return(true);
			_personHandler.Stub(x => x.Handle(scheduleDay.PersonPeriodId)).Return(personPart);
			_analyticsScheduleRepository.Stub(x => x.ShiftCategories()).Return(new List<IAnalyticsGeneric> { cat });
			_dateHandler.Stub(
				x =>
					x.Handle(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<DateOnly>.Is.Anything,
						Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything)).Return(null);
			_scheduleDayCountHandler.Stub(
				x =>
					x.Handle(Arg<ProjectionChangedEventScheduleDay>.Is.Anything, Arg<IAnalyticsFactSchedulePerson>.Is.Anything,
						Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(scheduleDayCountPart);

			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.DeleteFactSchedule(dateId, personPart.PersonId), y => y.Repeat.Times(2));
			_analyticsScheduleRepository.AssertWasNotCalled(
				x =>
					x.PersistFactScheduleRow(Arg<AnalyticsFactScheduleTime>.Is.Anything, Arg<AnalyticsFactScheduleDate>.Is.Anything,
						Arg<AnalyticsFactSchedulePerson>.Is.Anything));
			_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleDayCountRow(scheduleDayCountPart), y => y.Repeat.Times(1));
		}

		private IEnumerable<ProjectionChangedEventLayer> createLayers(DateTime startOfShift, IEnumerable<int> lengthCollection)
		{
			int accStart = 0;
			var layerList = new List<ProjectionChangedEventLayer>();

			foreach (int length in lengthCollection)
			{
				layerList.Add(
					new ProjectionChangedEventLayer
					{
						StartDateTime = startOfShift.AddMinutes(accStart),
						EndDateTime = startOfShift.AddMinutes(accStart).AddMinutes(length),
						ContractTime = new TimeSpan(0, 0, length),
						WorkTime = new TimeSpan(0, 0, length),
						DisplayColor = Color.Brown.ToArgb(),
						IsAbsence = true,
						IsAbsenceConfidential = true,
						Name = "Jonas",
						ShortName = "JN",
						PayloadId = Guid.NewGuid()
					}
					);
				accStart += length;
			}
			return layerList;
		}
	}
}
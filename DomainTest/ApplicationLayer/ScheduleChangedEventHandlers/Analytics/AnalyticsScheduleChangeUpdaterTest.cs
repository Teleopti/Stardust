using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsScheduleChangeUpdaterTest
	{
		private IIntervalLengthFetcher _intervalLengthFetcher;
		private AnalyticsScheduleChangeUpdater _target;
		private IAnalyticsFactScheduleTimeHandler _analyticsFactScheduleTimeHandler;
		private IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private IAnalyticsFactScheduleDateHandler _analyticsFactScheduleDateHandler;
		private IAnalyticsFactSchedulePersonHandler _analyticsFactSchedulePersonHandler;

		[SetUp]
		public void Setup()
		{
			_intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			_analyticsFactScheduleTimeHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleTimeHandler>();
			_analyticsFactScheduleDateHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDateHandler>();
			_analyticsFactSchedulePersonHandler = MockRepository.GenerateMock<IAnalyticsFactSchedulePersonHandler>();
			_analyticsScheduleRepository = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			_target = new AnalyticsScheduleChangeUpdater(
				_intervalLengthFetcher, 
				_analyticsFactScheduleTimeHandler,
				_analyticsFactScheduleDateHandler, 
				_analyticsFactSchedulePersonHandler, 
				_analyticsScheduleRepository);
		}

		[Test]
		public void ShouldKnowIntervalLength()
		{
			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			_target.Handle(new ProjectionChangedEvent{ScheduleDays = new Collection<ProjectionChangedEventScheduleDay>()});
			_intervalLengthFetcher.AssertWasCalled(x => x.IntervalLength);
		}

		[Test]
		public void ShouldCallPersistOnRepository()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start, new[] { 60, 15, 60 });
			var shift = new ProjectionChangedEventShift{StartDateTime = start, EndDateTime = start.AddHours(2).AddMinutes(15),Layers = list};
			var scheduleDay = new ProjectionChangedEventScheduleDay{Shift = shift};
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> {scheduleDay}
			};
			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			
			_target.Handle(@event);
			_analyticsScheduleRepository.AssertWasCalled(
				x => x.PersistFactScheduleRow(Arg<AnalyticsFactScheduleTime>.Is.Anything, Arg<AnalyticsFactScheduleDate>.Is.Anything,
						Arg<AnalyticsFactSchedulePerson>.Is.Anything), y => y.Repeat.Times(9));
			//_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleDayCountRow(Arg<AnalyticsFactScheduleDayCount>.Is.Anything), y => y.Repeat.Times(1));
		}

		[Test]
		public void ShouldGetLayerAndSendToRepository()
		{
			var start = new DateTime(2014, 12, 01, 8, 0, 0, DateTimeKind.Utc);
			var list = createLayers(start, new[] { 60, 15, 60 });
			var shift = new ProjectionChangedEventShift { StartDateTime = start, EndDateTime = start.AddHours(2).AddMinutes(15), Layers = list };
			var scheduleDay = new ProjectionChangedEventScheduleDay { Shift = shift };
			var @event = new ProjectionChangedEvent
			{
				ScheduleDays = new Collection<ProjectionChangedEventScheduleDay> { scheduleDay }
			};
			var timePart = new AnalyticsFactScheduleTime();
			var datePart = new AnalyticsFactScheduleDate();
			var personPart = new AnalyticsFactSchedulePerson();

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			_analyticsFactScheduleTimeHandler.Stub(x => x.Handle(Arg<ProjectionChangedEventLayer>.Is.Anything)).Return(timePart);
			_analyticsFactScheduleDateHandler.Stub(x => x.Handle(Arg<ProjectionChangedEventLayer>.Is.Anything)).Return(datePart);
			_analyticsFactSchedulePersonHandler.Stub(x => x.Handle(Arg<ProjectionChangedEventLayer>.Is.Anything)).Return(personPart);
			_target.Handle(@event);

			_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleRow(timePart, datePart, personPart), y => y.Repeat.Times(9));
			//_analyticsScheduleRepository.AssertWasCalled(x => x.PersistFactScheduleDayCountRow(retLayer), y => y.Repeat.Times(9));
			
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
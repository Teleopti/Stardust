using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsFactScheduleHandlerTest
	{
		IAnalyticsFactScheduleHandler _target;
		private IIntervalLengthFetcher _intervalLengthFetcher;
		private IAnalyticsFactScheduleDateHandler _dateHandler;
		private IAnalyticsFactScheduleTimeHandler _timeHandler;

		[SetUp]
		public void Setup()
		{
			_intervalLengthFetcher = MockRepository.GenerateMock<IIntervalLengthFetcher>();
			_dateHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleDateHandler>();
			_timeHandler = MockRepository.GenerateMock<IAnalyticsFactScheduleTimeHandler>();
			_target = new AnalyticsFactScheduleHandler(_intervalLengthFetcher, _dateHandler, _timeHandler);
		}

		[Test]
		public void ShouldReturnEmptyListIfNoShift()
		{
			var result = _target.AgentDaySchedule(new ProjectionChangedEventScheduleDay(), null, DateTime.Now, 1, 1);
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetIntervalLength()
		{
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = new DateTime(),
					EndDateTime = new DateTime()
				}
			};
			_target.AgentDaySchedule(scheduleDay, null, DateTime.Now, 1, 1);

			_intervalLengthFetcher.AssertWasCalled(x => x.IntervalLength);
		}

		[Test]
		public void ShouldReturnNullWhenDateCouldNotBeHandled()
		{
			var shiftStart = new DateTime(2015, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				}
			};
			scheduleDay.Shift.Layers = createLayers(shiftStart, new[] {60});

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			_dateHandler.Stub(
				x =>
					x.Handle(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<DateOnly>.Is.Anything,
						Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything)).Return(null);

			var result = _target.AgentDaySchedule(scheduleDay, null, DateTime.Now, 1, 1);
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGatherPartsForFactScheduleRow()
		{
			var shiftStart = new DateTime(2015, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var scheduleDay = new ProjectionChangedEventScheduleDay
			{
				Shift = new ProjectionChangedEventShift
				{
					StartDateTime = shiftStart,
					EndDateTime = shiftStart.AddHours(1)
				}
			};
			var datePart = new AnalyticsFactScheduleDate();
			var timePart = new AnalyticsFactScheduleTime();
			var personPart = new AnalyticsFactSchedulePerson();

			scheduleDay.Shift.Layers = createLayers(shiftStart, new[] { 60 });

			_intervalLengthFetcher.Stub(x => x.IntervalLength).Return(15);
			
			_dateHandler.Stub(
				x =>
					x.Handle(Arg<DateTime>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<DateOnly>.Is.Anything,
						Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<DateTime>.Is.Anything, Arg<int>.Is.Anything)).Return(datePart);

			_timeHandler.Stub(
				x =>
					x.Handle(Arg<ProjectionChangedEventLayer>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything,
						Arg<int>.Is.Anything)).Return(timePart);

			var result = _target.AgentDaySchedule(scheduleDay, personPart, DateTime.Now, 1, 1);

			result.Count.Should().Be.EqualTo(4);
			result[0].DatePart.Should().Be.SameInstanceAs(datePart);
			result[0].TimePart.Should().Be.SameInstanceAs(timePart);
			result[0].PersonPart.Should().Be.SameInstanceAs(personPart);
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

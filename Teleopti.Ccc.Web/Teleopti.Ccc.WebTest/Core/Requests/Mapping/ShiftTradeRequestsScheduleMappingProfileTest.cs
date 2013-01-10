﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeRequestsScheduleMappingProfileTest
	{
		private IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private IPerson _person;
		private IProjectionProvider _projectionProvider;
		private StubFactory _scheduleFactory;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void Setup()
		{
			_shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_scheduleFactory = new StubFactory();
			_timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_person = new Person {Name = new Name("John", "Doe")};
			_person.PermissionInformation.SetDefaultTimeZone(_timeZone);

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeRequestsScheduleMappingProfile(() => _shiftTradeRequestProvider, () => _projectionProvider)));
		}

		[Test]
		public void ShouldMapMyScheduledDayStartTime()
		{
			var startDate = new DateTime(2000, 1, 1, 8, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
		                                                }));
			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly(startDate));

			result.MyScheduleLayers.First().StartTimeText.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(startDate, _timeZone).ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapMyScheduledDayEndTime()
		{
			var endDate = new DateTime(2000, 1, 1, 11, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(endDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(endDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(endDate.AddHours(-3), endDate))
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly(endDate));

			result.MyScheduleLayers.First().EndTimeText.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(endDate, _timeZone).ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapMyScheduledDayLength()
		{
			var layerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly(layerPeriod.StartDateTime));

			result.MyScheduleLayers.First().LengthInMinutes.Should().Be.EqualTo(layerPeriod.ElapsedTime().TotalMinutes);
		}

		[Test]
		public void ShouldMapMyScheduledDayPayloadName()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			const string activtyName = "Phone";

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(activtyName)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly());

			result.MyScheduleLayers.First().Payload.Should().Be.EqualTo(activtyName);
		}

		[Test]
		public void ShouldMapMyScheduledDayPayloadColor()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			Color activtyColor = Color.Moccasin;

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(activtyColor)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly());

			result.MyScheduleLayers.First().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(activtyColor));
		}

		[Test]
		public void ShouldMapMyScheduledDayElapsedMinutesSinceShiftStart()
		{
			var layerPeriod1 = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc));
			var layerPeriod2 = new DateTimePeriod(new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 21, 15, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod1),
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod2)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly());

			var expectedValue = layerPeriod2.StartDateTime.Subtract(layerPeriod1.StartDateTime).TotalMinutes;
			result.MyScheduleLayers.Last().ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(expectedValue);
		}

		[Test]
		public void ShouldMapPossiblePersonsToTradeWith()
		{
			var possibleTradePersonLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc),
														   new DateTime(2013, 1, 1, 21, 40, 0, DateTimeKind.Utc));

			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(
				x => x.RetrievePossibleTradePersonsScheduleDay(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime))).Return(new[] { scheduleDay });
			_projectionProvider.Stub(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime));
			result.PossibleTradePersons.FirstOrDefault().Name.Should().Be.EqualTo(_person.Name.ToString());
			result.PossibleTradePersons.FirstOrDefault().ScheduleLayers.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapTimeLineStuff()
		{
			var possibleTradePersonLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc),
														   new DateTime(2013, 1, 1, 15, 40, 0, DateTimeKind.Utc));
			var myScheduleLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 11, 5, 0, DateTimeKind.Utc),
														   new DateTime(2013, 1, 1, 14, 0, 0, DateTimeKind.Utc));
			var possibleTradePerson = new Person { Name = new Name("Trade", "Victim") };
			possibleTradePerson.PermissionInformation.SetDefaultTimeZone(_timeZone);

			var myScheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			var possibleTradeScheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), possibleTradePerson);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(myScheduleLayerPeriod.StartDateTime))).Return(myScheduleDay);
			_shiftTradeRequestProvider.Stub(
				x => x.RetrievePossibleTradePersonsScheduleDay(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime))).Return(new[] { possibleTradeScheduleDay });
			
			_projectionProvider.Stub(p => p.Projection(myScheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(myScheduleLayerPeriod)
		                                                }));
			_projectionProvider.Stub(p => p.Projection(possibleTradeScheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime));
			
			result.TimeLineHours.Count().Should().Be.EqualTo(6);

			result.TimeLineHours.First().HourText.Should().Be.EqualTo(string.Empty);
			result.TimeLineHours.First().MinutesToDisplay.Should().Be.EqualTo(10);

			result.TimeLineHours.ElementAt(1).HourText.Should().Be.EqualTo("12");
			result.TimeLineHours.ElementAt(1).MinutesToDisplay.Should().Be.EqualTo(60);

			result.TimeLineHours.ElementAt(4).HourText.Should().Be.EqualTo("15");
			result.TimeLineHours.ElementAt(4).MinutesToDisplay.Should().Be.EqualTo(60);

			result.TimeLineHours.Last().HourText.Should().Be.EqualTo(string.Empty);
			result.TimeLineHours.Last().MinutesToDisplay.Should().Be.EqualTo(55);

			double expectedValue = possibleTradePersonLayerPeriod.EndDateTime.AddMinutes(15).Subtract(
											myScheduleLayerPeriod.StartDateTime.AddMinutes(-15)).TotalMinutes;
			result.TimeLineLengthInMinutes.Should().Be.EqualTo((int) expectedValue);
		}
	}
}

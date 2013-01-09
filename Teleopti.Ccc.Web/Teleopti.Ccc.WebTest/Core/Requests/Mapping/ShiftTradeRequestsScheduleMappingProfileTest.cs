using System;
using System.Drawing;
using System.Globalization;
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
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeRequestsScheduleMappingProfileTest
	{
		private IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private ILoggedOnUser _loggedOnUser;
		private IPerson _person;
		private IProjectionProvider _projectionProvider;
		private StubFactory _scheduleFactory;
		private TimeZoneInfo _timeZone;

		[SetUp]
		public void Setup()
		{
			_shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_scheduleFactory = new StubFactory();
			_timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_person = new Person();
			_person.PermissionInformation.SetDefaultTimeZone(_timeZone);

			_loggedOnUser.Stub(x => x.CurrentUser()).Return(_person);

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeRequestsScheduleMappingProfile(() => _shiftTradeRequestProvider, () => _loggedOnUser, () => _projectionProvider)));
		}

		[Test]
		public void ShouldMapMyScheduledDayStartTime()
		{
			var startDate = new DateTime(2000, 1, 1, 8, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveUserScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
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
			var scheduleDay = _scheduleFactory.ScheduleDayStub(endDate);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveUserScheduledDay(new DateOnly(endDate))).Return(scheduleDay);
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
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime());

			_shiftTradeRequestProvider.Stub(x => x.RetrieveUserScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
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
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime());
			const string activtyName = "Phone";

			_shiftTradeRequestProvider.Stub(x => x.RetrieveUserScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
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
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime());
			Color activtyColor = Color.Moccasin;

			_shiftTradeRequestProvider.Stub(x => x.RetrieveUserScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
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
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime());

			_shiftTradeRequestProvider.Stub(x => x.RetrieveUserScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod1),
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod2)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeRequestsScheduleViewModel>(new DateOnly());

			var expectedValue = layerPeriod2.StartDateTime.Subtract(layerPeriod1.StartDateTime).TotalMinutes;
			result.MyScheduleLayers.Last().ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(expectedValue);
		}
	}
}

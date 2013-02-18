using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeScheduleViewModelMappingProfileTest
	{
		private IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private IPerson _person;
		private IProjectionProvider _projectionProvider;
		private StubFactory _scheduleFactory;
		private IShiftTradeTimeLineHoursViewModelFactory _timelineFactory;
		private IUserCulture _userCulture;

		[SetUp]
		public void Setup()
		{
			_shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_timelineFactory = MockRepository.GenerateMock<IShiftTradeTimeLineHoursViewModelFactory>();
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));
			_scheduleFactory = new StubFactory();
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_person = new Person {Name = new Name("John", "Doe")};
			_person.PermissionInformation.SetDefaultTimeZone(timeZone);
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeScheduleViewModelMappingProfile(() => _shiftTradeRequestProvider, () => _projectionProvider, Depend.On(_timelineFactory), () => _userCulture)));
		}

		#region henke move out mapping of scheduleday
		[Test]
		public void ShouldMapScheduleDayTextFromName()
		{
			var startDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
		                                                }));
			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);
			result.Name.Should().Be.EqualTo(_person.Name.ToString());
		}

		[Test]
		public void ShouldMapMinutesSinceTimeLineStartFromScheduleDay()
		{
			var startDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
		                                                }));
			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);
			result.MinutesSinceTimeLineStart.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapScheduledDayStartTime()
		{
			var startDate = new DateTime(2000, 1, 1, 8, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
		                                                }));
			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);
			var expectedDate = TimeZoneHelper.ConvertFromUtc(startDate, _person.PermissionInformation.DefaultTimeZone());
			result.ScheduleLayers.First().Title.Should().Contain(expectedDate.ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapScheduledDayEndTime()
		{
			var endDate = new DateTime(2000, 1, 1, 11, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(endDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(endDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(endDate.AddHours(-3), endDate))
		                                                }));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);

			var expectedDate = TimeZoneHelper.ConvertFromUtc(endDate, _person.PermissionInformation.DefaultTimeZone());
			result.ScheduleLayers.First().Title.Should().Contain(expectedDate.ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapScheduledDayLength()
		{
			var layerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod)
		                                                }));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);

			result.ScheduleLayers.First().LengthInMinutes.Should().Be.EqualTo(layerPeriod.ElapsedTime().TotalMinutes);
		}

		[Test]
		public void ShouldMapScheduledDayPayloadName()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			const string activtyName = "Phone";

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(activtyName)
		                                                }));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);

			result.ScheduleLayers.First().Payload.Should().Be.EqualTo(activtyName);
		}

		[Test]
		public void ShouldMapScheduledDayPayloadColor()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			Color activtyColor = Color.Moccasin;

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(activtyColor)
		                                                }));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);

			result.ScheduleLayers.First().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(activtyColor));
		}

		[Test]
		public void ShouldMapScheduledDayElapsedMinutesSinceShiftStart()
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

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);

			var expectedValue = layerPeriod2.StartDateTime.Subtract(layerPeriod1.StartDateTime).TotalMinutes;
			result.ScheduleLayers.Last().ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(expectedValue);
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var theDayOff = new DayOffTemplate(new Description("my day off"));
			var personDayOff = new PersonDayOff(_person, new Scenario("scenario"), theDayOff, DateOnly.Today);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.DayOff, personDayOff);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub());

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);


			result.ScheduleLayers.Count().Should().Be.EqualTo(1);
			result.DayOffText.Should().Be.EqualTo(theDayOff.Description.Name);
			result.MinutesSinceTimeLineStart.Should().Be.EqualTo(15);

			var dayOffLayer = result.ScheduleLayers.First();
			dayOffLayer.Color.Should().Be.Empty();
			dayOffLayer.ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(0);
			dayOffLayer.LengthInMinutes.Should().Be.EqualTo(TimeSpan.FromHours(9).TotalMinutes);
		}

		[Test]
		public void ShouldMapAbsenceWithUnderlyingDayOff()
		{
			var period = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 1, 1, 17, 0, 0, DateTimeKind.Utc));
			var myDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.ContractDayOff,
																		PersonAbsenceFactory.CreatePersonAbsence(_person, new Scenario("sc"), period));
			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Stub(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(period, _person)
		                                                }));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(myDay);

			result.HasUnderlyingDayOff.Should().Be.True();
		}
		#endregion //henke move out

		[Test]
		public void ShouldMapMinutesSinceTimeLineStartSetWhenOnlyMySchedule()
		{
			var startDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
		                                                }));
			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(startDate));
			result.MySchedule.MinutesSinceTimeLineStart.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapMyScheduleTextNextToMyScheduledShift()
		{
			var startDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
		                                                }));
			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(startDate));
			result.MySchedule.Name.Should().Be.EqualTo(UserTexts.Resources.MySchedule);
		}

		[Test]
		public void ShouldMapMinutesSinceTimeLineStartSetWhenBothScheduleForMeAndTradeBuddy()
		{
			var buddy = new Person { Name = new Name("Buddy", "Bob") };
			var myStartDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var buddyStartDate = new DateTime(2000, 1, 1, 9, 30, 0, DateTimeKind.Utc);
			var myScheduleDay = _scheduleFactory.ScheduleDayStub(myStartDate, _person);
			var buddyScheduleDay = _scheduleFactory.ScheduleDayStub(buddyStartDate, buddy);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(myStartDate))).Return(myScheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(new DateOnly(buddyStartDate))).Return(new List<IScheduleDay>
			                                                                                                                  	{
			                                                                                                                  		buddyScheduleDay
			                                                                                                                  	});
			_projectionProvider.Expect(p => p.Projection(myScheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(myStartDate, myStartDate.AddHours(3)))
		                                                }));
			_projectionProvider.Expect(p => p.Projection(buddyScheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(buddyStartDate, buddyStartDate.AddHours(3)), buddy)
		                                                }));
			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(myStartDate));
			result.MySchedule.MinutesSinceTimeLineStart.Should().Be.EqualTo(45);
			result.PossibleTradePersons.First().MinutesSinceTimeLineStart.Should().Be.EqualTo(15);
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
			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(startDate));

			var expectedDate = TimeZoneHelper.ConvertFromUtc(startDate, _person.PermissionInformation.DefaultTimeZone());
			result.MySchedule.ScheduleLayers.First().Title.Should().Contain(expectedDate.ToString("HH:mm"));
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

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(endDate));

			var expectedDate = TimeZoneHelper.ConvertFromUtc(endDate, _person.PermissionInformation.DefaultTimeZone());
			result.MySchedule.ScheduleLayers.First().Title.Should().Contain(expectedDate.ToString("HH:mm"));
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

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(layerPeriod.StartDateTime));

			result.MySchedule.ScheduleLayers.First().LengthInMinutes.Should().Be.EqualTo(layerPeriod.ElapsedTime().TotalMinutes);
		}

		[Test]
		public void ShouldSetTitleInSwedish()
		{
			_userCulture.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));

			var date = new DateOnly(2000, 1, 1);
			var layerPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(date);

			result.MySchedule.ScheduleLayers.First().Title.Should().Be.EqualTo("11:00-14:00");
		}

		[Test]
		public void ShouldSetTitleInUs()
		{
			_userCulture.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("en-US")).Repeat.Any();

			var date = new DateOnly(2000, 1, 1);
			var layerPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(date);

			result.MySchedule.ScheduleLayers.First().Title.Should().Be.EqualTo("11:00 AM-2:00 PM");
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

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly());

			result.MySchedule.ScheduleLayers.First().Payload.Should().Be.EqualTo(activtyName);
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

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly());

			result.MySchedule.ScheduleLayers.First().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(activtyColor));
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

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly());

			var expectedValue = layerPeriod2.StartDateTime.Subtract(layerPeriod1.StartDateTime).TotalMinutes;
			result.MySchedule.ScheduleLayers.Last().ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(expectedValue);
		}

		[Test]
		public void ShouldMapMyDayOff()
		{
			var theDayOff = new DayOffTemplate(new Description("my day off"));
			var personDayOff = new PersonDayOff(_person, new Scenario("scenario"), theDayOff, DateOnly.Today);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.DayOff, personDayOff);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub());

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(DateOnly.Today);

			result.MySchedule.ScheduleLayers.Count().Should().Be.EqualTo(1);
			result.MySchedule.DayOffText.Should().Be.EqualTo(theDayOff.Description.Name);
			result.MySchedule.MinutesSinceTimeLineStart.Should().Be.EqualTo(15);

			var dayOffLayer = result.MySchedule.ScheduleLayers.First();
			dayOffLayer.Color.Should().Be.Empty();
			dayOffLayer.ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(0);
			dayOffLayer.LengthInMinutes.Should().Be.EqualTo(TimeSpan.FromHours(9).TotalMinutes);
		}

		[Test]
		public void ShouldMapMyAbsenceWithUnderlyingDayOff()
		{
			var period = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 1, 1, 17, 0, 0, DateTimeKind.Utc));
			var myDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.ContractDayOff,
																		PersonAbsenceFactory.CreatePersonAbsence(_person, new Scenario("sc"), period));
			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Stub(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(period, _person)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(DateOnly.Today);

			result.MySchedule.HasUnderlyingDayOff.Should().Be.True();
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
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod, _person)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime));
			result.PossibleTradePersons.FirstOrDefault().Name.Should().Be.EqualTo(_person.Name.ToString());
			result.PossibleTradePersons.FirstOrDefault().ScheduleLayers.Count().Should().Be.EqualTo(1);
		}

		[Test, Ignore("Henke: 20130215 I think we can move this test to a separate test and just make sure it calls the timelineviewmodelfactory")]
		public void ShouldMapTimeLineStuffWhenScheduleExist()
		{
			var possibleTradePersonLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc),
														   new DateTime(2013, 1, 1, 15, 40, 0, DateTimeKind.Utc));
			var myScheduleLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 11, 5, 0, DateTimeKind.Utc),
														   new DateTime(2013, 1, 1, 14, 0, 0, DateTimeKind.Utc));
			var possibleTradePerson = new Person { Name = new Name("Trade", "Victim") };

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
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod, possibleTradePerson)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime));
			
			result.TimeLineHours.Count().Should().Be.EqualTo(6);

			result.TimeLineHours.First().HourText.Should().Be.Empty();
			result.TimeLineHours.First().LengthInMinutesToDisplay.Should().Be.EqualTo(10);

			result.TimeLineHours.ElementAt(1).HourText.Should().Be.EqualTo("12");
			result.TimeLineHours.ElementAt(1).LengthInMinutesToDisplay.Should().Be.EqualTo(60);

			result.TimeLineHours.ElementAt(4).HourText.Should().Be.EqualTo("15");
			result.TimeLineHours.ElementAt(4).LengthInMinutesToDisplay.Should().Be.EqualTo(60);

			result.TimeLineHours.Last().HourText.Should().Be.EqualTo("16");
			result.TimeLineHours.Last().LengthInMinutesToDisplay.Should().Be.EqualTo(55);

			double expectedValue = possibleTradePersonLayerPeriod.EndDateTime.AddMinutes(15).Subtract(
											myScheduleLayerPeriod.StartDateTime.AddMinutes(-15)).TotalMinutes;
			result.TimeLineLengthInMinutes.Should().Be.EqualTo((int) expectedValue);
		}

		[Test, Ignore("Henke: 20130215 I think we can move this test to a separate test and just make sure it calls the timelineviewmodelfactory")]
		public void ShouldMapTimeLine8To17WhenNoExistingSchedule()
		{
			var day = _scheduleFactory.ScheduleDayStub();

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(day);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay>());

			_projectionProvider.Stub(p => p.Projection(day)).Return(_scheduleFactory.ProjectionStub());

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(DateOnly.Today);

			result.TimeLineHours.Count().Should().Be.EqualTo(11);
			result.TimeLineHours.First().HourText.Should().Be.Empty();
			result.TimeLineHours.First().LengthInMinutesToDisplay.Should().Be.EqualTo(15);
			result.TimeLineHours.ElementAt(1).HourText.Should().Be.EqualTo("8");
			result.TimeLineHours.Last().HourText.Should().Be.EqualTo("17");
			result.TimeLineHours.Last().LengthInMinutesToDisplay.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapPossibleTradePersonsDayOff()
		{
			var possibleTradePerson = new Person { Name = new Name("Trade", "Victim") };
			var theDayOff = new DayOffTemplate(new Description("a day off"));
			var tradeVictimDayOff = new PersonDayOff(possibleTradePerson, new Scenario("scenario"), theDayOff, DateOnly.Today);
			var myDay = _scheduleFactory.ScheduleDayStub();
			var tradeVictimDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, possibleTradePerson, SchedulePartView.DayOff, tradeVictimDayOff);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay> { tradeVictimDay });
			_projectionProvider.Expect(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(p => p.Projection(tradeVictimDay)).Return(_scheduleFactory.ProjectionStub());

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(DateOnly.Today);

			result.PossibleTradePersons.Count().Should().Be.EqualTo(1);
			result.PossibleTradePersons.First().MinutesSinceTimeLineStart.Should().Be.EqualTo(15);
			result.PossibleTradePersons.First().DayOffText.Should().Be.EqualTo(theDayOff.Description.Name);

			var dayOffLayer = result.PossibleTradePersons.First().ScheduleLayers.First();
			dayOffLayer.Color.Should().Be.Empty();
			dayOffLayer.ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(0);
			dayOffLayer.LengthInMinutes.Should().Be.EqualTo(TimeSpan.FromHours(9).TotalMinutes);
		}

		[Test]
		public void ShouldMapTimeLineWhenIHaveDayOffAndTradeVictimSchedule()
		{
			var myDayOff = new PersonDayOff(_person, new Scenario("scenario"), new DayOffTemplate(new Description("a day off")), DateOnly.Today);
			var myDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.DayOff, myDayOff);
			var tradeVictimDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, new Person());
			var possibleTradePersonLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 0, 0, DateTimeKind.Utc),
																	new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc));

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay> { tradeVictimDay });
			_projectionProvider.Expect(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Stub(p => p.Projection(tradeVictimDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod, tradeVictimDay.Person)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(DateOnly.Today);

			var expectedTimeLineLingth = possibleTradePersonLayerPeriod.EndDateTime.Subtract(possibleTradePersonLayerPeriod.StartDateTime).TotalMinutes;
			expectedTimeLineLingth += 30;
			result.TimeLineLengthInMinutes.Should().Be.EqualTo(expectedTimeLineLingth);
		}

		[Test]
		public void ShouldMapTradeVictimAbsenceWithUnderlyingDayOff()
		{
			var tradeVictim = new Person { Name = new Name("Trade", "Victim") };
			var period = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 1, 1, 17, 0, 0, DateTimeKind.Utc));
			var myDay = _scheduleFactory.ScheduleDayStub();
			var victimDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, tradeVictim, SchedulePartView.ContractDayOff,
														 PersonAbsenceFactory.CreatePersonAbsence(tradeVictim, new Scenario("sc"), period));
			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything)).Return(new List<IScheduleDay> {victimDay});
			_projectionProvider.Stub(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Stub(p => p.Projection(victimDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(period, tradeVictim)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(DateOnly.Today);

			result.PossibleTradePersons.First().HasUnderlyingDayOff.Should().Be.True();
		}
	}
}

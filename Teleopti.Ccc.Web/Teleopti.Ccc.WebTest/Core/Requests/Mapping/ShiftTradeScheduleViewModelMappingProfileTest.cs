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
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
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
		private IUserCulture _userCulture;
		private IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private IUserTimeZone _userTimeZone;

		[SetUp]
		public void Setup()
		{
			_userTimeZone = MockRepository.GenerateStub<IUserTimeZone>();
			setUserTimeZoneTo(TimeZoneInfo.Utc);
			_shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var userCultureStub = MockRepository.GenerateStub<IUserCulture>();
			var createHourText = new CreateHourText(userCultureStub, new SpecificTimeZone(timeZone));
			var timeLineHoursFactory = new ShiftTradeTimeLineHoursViewModelFactory(createHourText);
			_person = new Person {Name = new Name("John", "Doe")};
			_person.PermissionInformation.SetDefaultTimeZone(timeZone);
			var userTimeZoneStub = MockRepository.GenerateStub<IUserTimeZone>();
			userTimeZoneStub.Expect(u => u.TimeZone()).Repeat.Any().Return(timeZone);
			_possibleShiftTradePersonsProvider = MockRepository.GenerateMock<IPossibleShiftTradePersonsProvider>();

			_scheduleFactory = new StubFactory();
			
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeScheduleViewModelMappingProfile(_shiftTradeRequestProvider, _projectionProvider, timeLineHoursFactory, _userCulture, _possibleShiftTradePersonsProvider,_userTimeZone)));
		}

	
		[Test]
		public void ShouldMapScheduleDayTextFromName()
		{
			var startDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
		                                                }));
			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);
			var expectedDate = TimeZoneHelper.ConvertFromUtc(startDate, _userTimeZone.TimeZone());
			result.ScheduleLayers.First().Title.Should().Contain(expectedDate.ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapScheduledDayEndTime()
		{
			var endDate = new DateTime(2000, 1, 1, 11, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(endDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(endDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(endDate.AddHours(-3), endDate))
		                                                }));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);

			var expectedDate = TimeZoneHelper.ConvertFromUtc(endDate, _userTimeZone.TimeZone());
			result.ScheduleLayers.First().Title.Should().Contain(expectedDate.ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapScheduledDayLength()
		{
			var layerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(activtyName)
		                                                }));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);

			result.ScheduleLayers.First().Payload.Should().Be.EqualTo(activtyName);
		}

		[Test]
		public void ShouldMapDayoffDate()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(2000,1,1), _person);
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(new Scenario("d"), _person, new DateOnly(2000,1,1), new DayOffTemplate()));
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new IVisualLayer[0]));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(scheduleDay);
			result.MinutesSinceTimeLineStart.Should().Be.IncludedIn(0, 60*24);
			result.StartTimeUtc.Should().Be.IncludedIn(new DateTime(1999, 12, 31), new DateTime(2000,1,2));
		}

		[Test]
		public void ShouldMapScheduledDayPayloadColor()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			Color activtyColor = Color.Moccasin;

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(new Scenario("scenario"), _person, DateOnly.Today, theDayOff);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.DayOff, personDayOff);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Stub(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(period, _person)
		                                                }));

			var result = Mapper.Map<IScheduleDay, ShiftTradePersonScheduleViewModel>(myDay);

			result.HasUnderlyingDayOff.Should().Be.True();
		}

		[Test]
		public void ShouldMapMinutesSinceTimeLineStartSetWhenOnlyMySchedule()
		{
			var startDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(startDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(new DateOnly(buddyStartDate), new List<IPerson>{buddy})).Return(new List<IScheduleDay>
			                                                                                                                  	{
			                                                                                                                  		buddyScheduleDay
			                                                                                                                  	});
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(new DateOnly(buddyStartDate))).Return(new List<IPerson> { buddy });

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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
		                                                }));
			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(startDate));

			var expectedDate = TimeZoneHelper.ConvertFromUtc(startDate, _userTimeZone.TimeZone());
			result.MySchedule.ScheduleLayers.First().Title.Should().Contain(expectedDate.ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapMyScheduledDayEndTime()
		{
			var endDate = new DateTime(2000, 1, 1, 11, 15, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(endDate, _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(endDate))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(endDate.AddHours(-3), endDate))
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(endDate));

			var expectedDate = TimeZoneHelper.ConvertFromUtc(endDate, _userTimeZone.TimeZone());
			result.MySchedule.ScheduleLayers.First().Title.Should().Contain(expectedDate.ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapMyScheduledDayLength()
		{
			var layerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(date);

			result.MySchedule.ScheduleLayers.First().Title.Should().Be.EqualTo("10:00-13:00");
		}

		[Test]
		public void ShouldSetTitleInUs()
		{
			_userCulture.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("en-US")).Repeat.Any();

			var date = new DateOnly(2000, 1, 1);
			var layerPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(date);

			result.MySchedule.ScheduleLayers.First().Title.Should().Be.EqualTo("10:00 AM-1:00 PM");
		}

		[Test]
		public void ShouldMapMyScheduledDayPayloadName()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			const string activtyName = "Phone";

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(new Scenario("scenario"), _person, DateOnly.Today, theDayOff);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.DayOff, personDayOff);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());
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
			var possibleTradePersons = new List<IPerson> {_person};
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(
				x => x.RetrievePossibleTradePersonsScheduleDay(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime), possibleTradePersons)).Return(new[] { scheduleDay });
			_projectionProvider.Stub(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod, _person)
		                                                }));
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(Arg<DateOnly>.Is.Anything)).Return(possibleTradePersons);

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime));
			result.PossibleTradePersons.First().Name.Should().Be.EqualTo(_person.Name.ToString());
			result.PossibleTradePersons.First().ScheduleLayers.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapPersonId()
		{
			var personId = Guid.NewGuid();
			_person.SetId(personId);
			var possibleTradePersonLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc),
												 new DateTime(2013, 1, 1, 21, 40, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			var possibleTradePersons = new List<IPerson> { _person };

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(
	            x => x.RetrievePossibleTradePersonsScheduleDay(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime), possibleTradePersons)).Return(new[] { scheduleDay });
			_projectionProvider.Stub(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod, _person)
		                                                }));
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(Arg<DateOnly>.Is.Anything)).Return(possibleTradePersons);

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime));
			result.PossibleTradePersons.First().PersonId.Should().Be.EqualTo(personId);
		}

		[Test]
		public void ShouldMapTimeLineStuffWhenScheduleExist()
		{
			var possibleTradePersonLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc),
														   new DateTime(2013, 1, 1, 15, 40, 0, DateTimeKind.Utc));
			var myScheduleLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 11, 5, 0, DateTimeKind.Utc),
														   new DateTime(2013, 1, 1, 14, 0, 0, DateTimeKind.Utc));
			var possibleTradePerson = new Person { Name = new Name("Trade", "Victim") };
			var possibleTradePersons = new[] {possibleTradePerson};

			var myScheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			var possibleTradeScheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), possibleTradePerson);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(myScheduleLayerPeriod.StartDateTime))).Return(myScheduleDay);
			_shiftTradeRequestProvider.Stub(
				x => x.RetrievePossibleTradePersonsScheduleDay(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime), possibleTradePersons)).Return(new[] { possibleTradeScheduleDay });
			
			_projectionProvider.Stub(p => p.Projection(myScheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(myScheduleLayerPeriod)
		                                                }));
			_projectionProvider.Stub(p => p.Projection(possibleTradeScheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod, possibleTradePerson)
		                                                }));
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(new DateOnly(possibleTradePersonLayerPeriod.StartDateTime))).Return(possibleTradePersons);

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

		[Test]
		public void ShouldMapTimeLine8To17WhenNoExistingSchedule()
		{
			var day = _scheduleFactory.ScheduleDayStub();

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(day);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());

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
			var date = DateOnly.Today;
			var possibleTradePerson = new Person { Name = new Name("Trade", "Victim") };
			var possibleTradePersons = new[] {possibleTradePerson};
			var theDayOff = new DayOffTemplate(new Description("a day off"));
			var tradeVictimDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(new Scenario("scenario"), possibleTradePerson, date, theDayOff);
			var myDay = _scheduleFactory.ScheduleDayStub();
			var tradeVictimDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, possibleTradePerson, SchedulePartView.DayOff, tradeVictimDayOff);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(date, possibleTradePersons)).Return(new List<IScheduleDay> { tradeVictimDay });
			_projectionProvider.Expect(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(p => p.Projection(tradeVictimDay)).Return(_scheduleFactory.ProjectionStub());
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(Arg<DateOnly>.Is.Anything)).Return(possibleTradePersons);

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(date);

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
			var myDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(new Scenario("scenario"), _person, DateOnly.Today, new DayOffTemplate(new Description("a day off")));
			var myDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.DayOff, myDayOff);
			var tradeVictimDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, new Person());
			var possibleTradePersonLayerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 0, 0, DateTimeKind.Utc),
																	new DateTime(2013, 1, 1, 12, 0, 0, DateTimeKind.Utc));

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay> { tradeVictimDay });
			_projectionProvider.Expect(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Stub(p => p.Projection(tradeVictimDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(possibleTradePersonLayerPeriod, tradeVictimDay.Person)
		                                                }));
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(Arg<DateOnly>.Is.Anything)).Return(new List<IPerson> { tradeVictimDay.Person });

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(DateOnly.Today);

			var expectedTimeLineLingth = possibleTradePersonLayerPeriod.EndDateTime.Subtract(possibleTradePersonLayerPeriod.StartDateTime).TotalMinutes;
			expectedTimeLineLingth += 30;
			result.TimeLineLengthInMinutes.Should().Be.EqualTo(expectedTimeLineLingth);
		}

		[Test]
		public void ShouldMapTradeVictimsWithNoSchedule()
		{
			var tradeVictim = new Person { Name = new Name("Trade", "Victim") };
			tradeVictim.SetId(Guid.NewGuid());
			var date = new DateOnly(2000, 1, 1);
			var myDay = _scheduleFactory.ScheduleDayStub(date, tradeVictim);
			var tradeVictims = new[] {tradeVictim};

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(date)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(date, tradeVictims)).Return(new List<IScheduleDay>());
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(date)).Return(tradeVictims);
			_projectionProvider.Expect(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub());

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(date).PossibleTradePersons.FirstOrDefault(model => model.PersonId == tradeVictim.Id);
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapTradeVictimAbsenceWithUnderlyingDayOff()
		{
			var date = DateOnly.Today;
			var tradeVictim = new Person { Name = new Name("Trade", "Victim") };
			var tradeVictims = new[] {tradeVictim};
			var period = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 1, 1, 17, 0, 0, DateTimeKind.Utc));
			var myDay = _scheduleFactory.ScheduleDayStub();
			var victimDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, tradeVictim, SchedulePartView.ContractDayOff,
														 PersonAbsenceFactory.CreatePersonAbsence(tradeVictim, new Scenario("sc"), period));
			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(Arg<DateOnly>.Is.Anything)).Return(myDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(date, tradeVictims)).Return(new List<IScheduleDay> { victimDay });
			_projectionProvider.Stub(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Stub(p => p.Projection(victimDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(period, tradeVictim)
		                                                }));
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(date)).Return(tradeVictims);

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(date);

			result.PossibleTradePersons.First().HasUnderlyingDayOff.Should().Be.True();
		}

		[Test]
		public void ShiftStart_WhenVictimHasAnotherTimeZoneButSameTimeInUtc_ShouldBeSameAsTheUsers()
		{
			var startDateUtc = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var theDate = new DateOnly(startDateUtc);

			var userTimeZone = TimeZoneInfo.CreateCustomTimeZone("Somewhere", TimeSpan.FromMinutes(0), "Somewhere", "Somewhere");
			_person.PermissionInformation.SetDefaultTimeZone(userTimeZone);
			createTradeableShift(startDateUtc, startDateUtc.AddHours(3));

			var victim = PersonFactory.CreatePerson("ShiftTrade", "Victim");
			var victimsTimeZone = TimeZoneInfo.CreateCustomTimeZone("Göteborg", TimeSpan.FromMinutes(30), "Göteborg", "Göteborg");
			victim.PermissionInformation.SetDefaultTimeZone(victimsTimeZone);
			createAPossibleShiftTrade(victim, startDateUtc, startDateUtc.AddHours(3));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(theDate);

			var minutesSinceTimelineStartForMyShift = result.MySchedule.MinutesSinceTimeLineStart;
			var minutesSinceTimelineStartForVictimsShift = result.PossibleTradePersons.First().MinutesSinceTimeLineStart;

			Assert.That(minutesSinceTimelineStartForMyShift, Is.EqualTo(minutesSinceTimelineStartForVictimsShift));
		}

		[Test]
		public void ShiftTradePersonSchedules_Always_ShouldBeInTheLoggedOnUsersTimeZone()
		{
			_userCulture.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));
			var personTimeZone = createTimeZoneInfo(TimeSpan.FromMinutes(10));
			_person.PermissionInformation.SetDefaultTimeZone(personTimeZone);
			setUserTimeZoneTo(createTimeZoneInfo(TimeSpan.FromMinutes(20)));

			var date = new DateOnly(2000, 1, 1);
			var layerPeriod = new DateTimePeriod(new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2000, 1, 1, 13, 0, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(new DateOnly(layerPeriod.StartDateTime))).Return(scheduleDay);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(Arg<DateOnly>.Is.Anything, Arg<IEnumerable<IPerson>>.Is.Anything)).Return(new List<IScheduleDay>());

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(layerPeriod)
		                                                }));

			var result = Mapper.Map<DateOnly, ShiftTradeScheduleViewModel>(date);

			result.MySchedule.ScheduleLayers.First().Title.Should().Be.EqualTo("10:20-13:20");
		}

		private void createAPossibleShiftTrade(IPerson person, DateTime scheduleStartsUtc, DateTime scheduleEndsUtc)
		{
			var theDate = new DateOnly(scheduleStartsUtc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(scheduleStartsUtc, person);
			var possibleTradeVictims = new List<IPerson>() { person };
			_possibleShiftTradePersonsProvider.Expect(x => x.RetrievePersons(theDate)).Return(possibleTradeVictims);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradePersonsScheduleDay(theDate, possibleTradeVictims)).Return(new List<IScheduleDay>() { scheduleDay });
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(scheduleStartsUtc, scheduleEndsUtc))
		                                                }));
		}

		private void createTradeableShift(DateTime scheduleStartsUtc, DateTime scheduleEndsUtc)
		{
			var theDate = new DateOnly(scheduleStartsUtc);
			var myScheduleDay = _scheduleFactory.ScheduleDayStub(scheduleStartsUtc, _person);
			_shiftTradeRequestProvider.Stub(x => x.RetrieveMyScheduledDay(theDate)).Return(myScheduleDay);
			_projectionProvider.Expect(p => p.Projection(myScheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
		                                                {
		                                                    _scheduleFactory.VisualLayerStub(new DateTimePeriod(scheduleStartsUtc, scheduleEndsUtc))
		                                                }));
		}

		private static TimeZoneInfo createTimeZoneInfo(TimeSpan diffFromUtc)
		{
			return TimeZoneInfo.CreateCustomTimeZone("Somewhere", diffFromUtc, "Somewhere", "Somewhere");
		}

		private void setUserTimeZoneTo(TimeZoneInfo timeZoneInfo)
		{
			_userTimeZone.BackToRecord(BackToRecordOptions.All);
			_userTimeZone.Expect(t => t.TimeZone()).Return(timeZoneInfo).Repeat.Any();
			_userTimeZone.Replay();
		
		}
	}
}

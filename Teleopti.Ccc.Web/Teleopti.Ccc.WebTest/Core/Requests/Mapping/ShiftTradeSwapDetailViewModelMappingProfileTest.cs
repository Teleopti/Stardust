using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeSwapDetailViewModelMappingProfileTest
	{
		private DateTime _dateFrom;
		private DateTime _dateTo;
		private StubFactory _scheduleFactory;
		private IShiftTradeTimeLineHoursViewModelFactory _timeLineFactory;
		private IProjectionProvider _projectionProvider;
		private Person _fromPerson;
		private Person _toPerson;
		private IPerson _person;
		private IUserCulture _userCulture;
		private IUserTimeZone _userTimeZone;

		[SetUp]
		public void Setup()
		{
			_fromPerson = new Person { Name = new Name("From", "Person"), };
			_toPerson = new Person { Name = new Name("To", "Person") };
			_scheduleFactory = new StubFactory();
			_timeLineFactory = MockRepository.GenerateStub<IShiftTradeTimeLineHoursViewModelFactory>();
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_dateFrom = new DateTime(2001, 12, 12, 0, 0, 0, DateTimeKind.Utc);
			_dateTo = new DateTime(2001, 12, 13, 0, 0, 0, DateTimeKind.Utc);

			_userTimeZone = MockRepository.GenerateStub<IUserTimeZone>();
			setUserTimeZoneTo(TimeZoneInfo.Utc);
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_person = new Person { Name = new Name("John", "Doe") };
			_person.PermissionInformation.SetDefaultTimeZone(timeZone);

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new ShiftTradeSwapDetailViewModelMappingProfile(_timeLineFactory, _projectionProvider, _userCulture, _userTimeZone)));
		}

		[Test]
		public void CreateScheduleViewModelsFromMapper()
		{
			var profileForProbing = new MappingProfileForProbing<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>();

			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();

			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();

			var shiftTradePersonScheduleViewModelStub = new ShiftTradeEditPersonScheduleViewModel();
			profileForProbing.Result = shiftTradePersonScheduleViewModelStub;

			Mapper.AddProfile(profileForProbing);

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			var result = Mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest.ShiftTradeSwapDetails.First());
			Assert.That(profileForProbing.HasBeenMappedFrom(scheduleDayFrom), "The mapper should have been called for the sheduleday From");
			Assert.That(profileForProbing.HasBeenMappedFrom(scheduleDayTo), "The mapper should have been called for the sheduleday To");
			Assert.That(result.To, Is.EqualTo(shiftTradePersonScheduleViewModelStub), "Should have been set from the mapper (we are using the same result for To and For");
			Assert.That(result.To, Is.EqualTo(shiftTradePersonScheduleViewModelStub), "Should have been set from the mapper (we are using the same result for To and For");
		}

		[Test]
		public void CreateTimelineWhenOneNightShift()
		{
			Mapper.AddProfile(new MappingProfileForProbing<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>());
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(23);
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub(new DateTimePeriod(expectedStart.AddHours(2), expectedEnd));
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedStart.AddHours(1))));
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart.AddHours(2), expectedEnd)));

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			Mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest.ShiftTradeSwapDetails.First());

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenToScheduleDayIsEmpty()
		{
			Mapper.AddProfile(new MappingProfileForProbing<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>());
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(10);
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub(new DateTimePeriod(expectedStart, expectedEnd));
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedEnd)));
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub());

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			Mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest.ShiftTradeSwapDetails.First());

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenFromScheduleDayIsEmpty()
		{
			Mapper.AddProfile(new MappingProfileForProbing<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>());
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(10);
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedEnd)));

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			Mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest.ShiftTradeSwapDetails.First());

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenBothScheduleDayIsEmpty()
		{
			Mapper.AddProfile(new MappingProfileForProbing<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>());
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub());

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			Mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(shiftTradeRequest.ShiftTradeSwapDetails.First());

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(
				new DateTimePeriod(shiftTradeRequest.Period.StartDateTime.AddHours(-1), shiftTradeRequest.Period.EndDateTime.AddHours(1))));
		}

		[Test]
		public void CreateTimelineBasedOnShiftTradePeriodIfNoSchedulesExists()
		{
			var timeLineHoursViewModelFactory = MockRepository.GenerateStrictMock<IShiftTradeTimeLineHoursViewModelFactory>();

			Mapper.Reset();

			Mapper.Initialize(
				c =>
				c.AddProfile(new ShiftTradeSwapDetailViewModelMappingProfile(timeLineHoursViewModelFactory, _projectionProvider,
																			  _userCulture, _userTimeZone)));
			AddNeededMappingProfiles();

			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			
			var shiftTrade = CreateShiftTrade(from, to, null,null);
			var expectedTimelinePeriod = shiftTrade.Period;

			var timelineHours = new List<ShiftTradeTimeLineHoursViewModel>() { new ShiftTradeTimeLineHoursViewModel(), new ShiftTradeTimeLineHoursViewModel() };
			timeLineHoursViewModelFactory.Expect(s => s.CreateTimeLineHours(expectedTimelinePeriod)).Return(timelineHours);

			var result = Mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(shiftTrade.ShiftTradeSwapDetails.First());

			timeLineHoursViewModelFactory.VerifyAllExpectations();

			Assert.That(result.TimeLineHours, Is.EqualTo(timelineHours));
			Assert.That(result.TimeLineStartDateTime, Is.EqualTo(expectedTimelinePeriod.StartDateTime));
		}

		[Test]
		public void CreateEmptyScheduleViewModelsIfNoSchedulesExists()
		{
			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			var shiftTrade = CreateShiftTrade(from, to, null, null);

			var result = Mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(shiftTrade.ShiftTradeSwapDetails.First());

			Assert.That(result.To, Is.Not.Null);
			Assert.That(result.From, Is.Not.Null);
		}

		[Test]
		public void Name_WhenNoScheduleExists_ShouldBeSet()
		{
			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			var shiftTrade = CreateShiftTrade(from, to, null, null);

			var result = Mapper.Map<IShiftTradeSwapDetail, ShiftTradeSwapDetailsViewModel>(shiftTrade.ShiftTradeSwapDetails.First());

			result.PersonFrom
				.Should().Be.EqualTo(_fromPerson.Name.ToString());
			result.PersonTo
				.Should().Be.EqualTo(_toPerson.Name.ToString());
		}


		[Test]
		public void ShouldMapScheduleDayTextFromName()
		{
			var startDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
														{
															_scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
														}));
			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);
			result.Name.Should().Be.EqualTo(_person.Name.ToString());
		}

		[Test]
		public void ShouldMapMinutesSinceTimeLineStartFromScheduleDay()
		{
			var startDate = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
														{
															_scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, startDate.AddHours(3)))
														}));
			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);
			result.MinutesSinceTimeLineStart.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapScheduledDayTimes()
		{
			var startDate = new DateTime(2000, 1, 1, 8, 15, 0, DateTimeKind.Utc);
			var endDate = startDate.AddHours(3);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(startDate, _person);

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
														{
															_scheduleFactory.VisualLayerStub(new DateTimePeriod(startDate, endDate))
														}));
			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);

			var expectedStartTime = TimeZoneHelper.ConvertFromUtc(startDate, _userTimeZone.TimeZone());
			var expectedEndTime = TimeZoneHelper.ConvertFromUtc(startDate, _userTimeZone.TimeZone());

			result.ScheduleLayers.First().TitleTime.Should().Contain(expectedStartTime.ToString("HH:mm"));
			result.ScheduleLayers.First().TitleTime.Should().Contain(expectedEndTime.ToString("HH:mm"));
		}

		[Test]
		public void ShouldMapScheduledDayLength()
		{
			var layerPeriod = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 15, 0, DateTimeKind.Utc),
												 new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
														{
															_scheduleFactory.VisualLayerStub(layerPeriod)
														}));

			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);

			result.ScheduleLayers.First().LengthInMinutes.Should().Be.EqualTo(layerPeriod.ElapsedTime().TotalMinutes);
		}

		[Test]
		public void ShouldMapScheduledDayPayloadName()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			const string activtyName = "Phone";

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
														{
															_scheduleFactory.VisualLayerStub(activtyName)
														}));

			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);

			result.ScheduleLayers.First().Payload.Should().Be.EqualTo(activtyName);
		}

		[Test]
		public void ShouldMapDayoffDate()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(2000, 1, 1), _person);
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(new Scenario("d"), _person, new DateOnly(2000, 1, 1), new DayOffTemplate()));
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new IVisualLayer[0]));

			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);
			result.MinutesSinceTimeLineStart.Should().Be.IncludedIn(0, 60 * 24);
			result.StartTimeUtc.Should().Be.IncludedIn(new DateTime(1999, 12, 31), new DateTime(2000, 1, 2));
		}

		[Test]
		public void ShouldMapScheduledDayPayloadColor()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);
			Color activtyColor = Color.Moccasin;

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
														{
															_scheduleFactory.VisualLayerStub(activtyColor)
														}));

			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);

			result.ScheduleLayers.First().Color.Should().Be.EqualTo(ColorTranslator.ToHtml(activtyColor));
		}

		[Test]
		public void ShouldMapScheduledDayElapsedMinutesSinceShiftStart()
		{
			var layerPeriod1 = new DateTimePeriod(new DateTime(2013, 1, 1, 8, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc));
			var layerPeriod2 = new DateTimePeriod(new DateTime(2013, 1, 1, 20, 15, 0, DateTimeKind.Utc), new DateTime(2013, 1, 1, 21, 15, 0, DateTimeKind.Utc));
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(), _person);

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new[]
														{
															_scheduleFactory.VisualLayerStub(layerPeriod1),
															_scheduleFactory.VisualLayerStub(layerPeriod2)
														}));

			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);

			var expectedValue = layerPeriod2.StartDateTime.Subtract(layerPeriod1.StartDateTime).TotalMinutes;
			result.ScheduleLayers.Last().ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(expectedValue);
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var theDayOff = new DayOffTemplate(new Description("my day off"));
			var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(new Scenario("scenario"), _person, DateOnly.Today, theDayOff);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.DayOff, personDayOff);

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub());

			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(scheduleDay);

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
			_projectionProvider.Stub(p => p.Projection(myDay)).Return(_scheduleFactory.ProjectionStub(new[]
														{
															_scheduleFactory.VisualLayerStub(period, _person)
														}));

			var result = Mapper.Map<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>(myDay);

			result.HasUnderlyingDayOff.Should().Be.True();
		}


		private void AddNeededMappingProfiles()
		{
			var profileStubForHandlingScheduleDays = new MappingProfileForProbing<IScheduleDay, ShiftTradeEditPersonScheduleViewModel>
				{
					Result = new ShiftTradeEditPersonScheduleViewModel()
				};
			Mapper.AddProfile(profileStubForHandlingScheduleDays);
		}

		private  IShiftTradeRequest CreateShiftTrade(DateTime dateFrom, DateTime dateTo, IScheduleDay scheduleDayFrom, IScheduleDay scheduleDayTo)
		{
			var details = new List<IShiftTradeSwapDetail>();

			var detail = new ShiftTradeSwapDetail(_fromPerson, _toPerson, new DateOnly(dateFrom), new DateOnly(dateTo))
									 {
										 SchedulePartFrom = scheduleDayFrom,
										 SchedulePartTo = scheduleDayTo
									 };
			details.Add(detail);

			var shiftTrade = new ShiftTradeRequest(details);

			return shiftTrade;
		}

		private void setUserTimeZoneTo(TimeZoneInfo timeZoneInfo)
		{
			_userTimeZone.BackToRecord(BackToRecordOptions.All);
			_userTimeZone.Expect(t => t.TimeZone()).Return(timeZoneInfo).Repeat.Any();
			_userTimeZone.Replay();
		}
	}
}
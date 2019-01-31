using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;


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
		private IPersonNameProvider _personNameProvider;
		private ShiftTradeSwapDetailViewModelMapper target;
		private NameFormatSettings _nameFormatSettings;

		[SetUp]
		public void Setup()
		{
			_fromPerson = new Person().WithName(new Name("From", "Person"));
			_toPerson = new Person().WithName(new Name("To", "Person"));
			_scheduleFactory = new StubFactory();
			_timeLineFactory = MockRepository.GenerateStub<IShiftTradeTimeLineHoursViewModelFactory>();
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_dateFrom = new DateTime(2001, 12, 12, 0, 0, 0, DateTimeKind.Utc);
			_dateTo = new DateTime(2001, 12, 13, 0, 0, 0, DateTimeKind.Utc);
			_nameFormatSettings = new NameFormatSettings {NameFormatId = 0};

			_userTimeZone = MockRepository.GenerateStub<IUserTimeZone>();
			setUserTimeZoneTo(TimeZoneInfo.Utc);
			_userCulture = MockRepository.GenerateMock<IUserCulture>();
			_userCulture.Expect(c => c.GetCulture()).Return(CultureInfo.GetCultureInfo("sv-SE"));
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_person = new Person().WithName(new Name("John", "Doe"));
			_person.PermissionInformation.SetDefaultTimeZone(timeZone);

			var nameFormatSettingsPersisterAndProvider = MockRepository.GenerateStub<ISettingsPersisterAndProvider<NameFormatSettings>>();
			nameFormatSettingsPersisterAndProvider.Stub(x => x.Get()).Return(_nameFormatSettings);
			_personNameProvider = new PersonNameProvider(nameFormatSettingsPersisterAndProvider);

			var loggedOnUser = MockRepository.GenerateStub<ILoggedOnUser>();
			_toPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
			loggedOnUser.Stub(x => x.CurrentUser()).Return(_toPerson);

			target = new ShiftTradeSwapDetailViewModelMapper(_timeLineFactory,_projectionProvider,_userCulture,_userTimeZone,_personNameProvider, loggedOnUser);
		}

		[Test]
		public void CreateScheduleViewModelsFromMapper()
		{
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();

			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			
			_projectionProvider.Stub(x => x.Projection(scheduleDayFrom)).Return(new VisualLayerCollection(new IVisualLayer[0], new ProjectionPayloadMerger()));
			_projectionProvider.Stub(x => x.Projection(scheduleDayTo)).Return(new VisualLayerCollection(new IVisualLayer[0], new ProjectionPayloadMerger()));

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			var result = target.Map(shiftTradeRequest.ShiftTradeSwapDetails.First(),_nameFormatSettings);

			Assert.That(result.From, Is.Not.Null, "Should have been set from the mapper (we are using the same result for To and For");
			Assert.That(result.To, Is.Not.Null, "Should have been set from the mapper (we are using the same result for To and For");
		}

		[Test]
		public void CreateTimelineWhenOneNightShift()
		{
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(23);
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedStart.AddHours(1))));
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart.AddHours(2), expectedEnd)));

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			target.Map(shiftTradeRequest.ShiftTradeSwapDetails.First(), _nameFormatSettings);

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenToScheduleDayIsEmpty()
		{
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(10);
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedEnd)));
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub());

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			target.Map(shiftTradeRequest.ShiftTradeSwapDetails.First(), _nameFormatSettings);

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenFromScheduleDayIsEmpty()
		{
			var expectedStart = new DateTime(2000, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			var expectedEnd = expectedStart.AddHours(10);
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub(new DateTimePeriod(expectedStart, expectedEnd)));

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			target.Map(shiftTradeRequest.ShiftTradeSwapDetails.First(), _nameFormatSettings);

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(new DateTimePeriod(expectedStart.AddHours(-1), expectedEnd.AddHours(1))));
		}

		[Test]
		public void CreateTimelineWhenBothScheduleDayIsEmpty()
		{
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub();
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub();
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub());

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			target.Map(shiftTradeRequest.ShiftTradeSwapDetails.First(), _nameFormatSettings);

			_timeLineFactory.AssertWasCalled(x => x.CreateTimeLineHours(
				new DateTimePeriod(shiftTradeRequest.Period.StartDateTime.AddHours(-1), shiftTradeRequest.Period.EndDateTime.AddHours(1))));
		}

		[Test]
		public void CreateTimelineBasedOnShiftTradePeriodIfNoSchedulesExists()
		{
			var timeLineHoursViewModelFactory = MockRepository.GenerateStrictMock<IShiftTradeTimeLineHoursViewModelFactory>();

			target = new ShiftTradeSwapDetailViewModelMapper(timeLineHoursViewModelFactory, _projectionProvider, _userCulture, _userTimeZone, _personNameProvider, null);
			
			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);
			
			var shiftTrade = CreateShiftTrade(from, to, null, null);
			var expectedTimelinePeriod = shiftTrade.Period;

			var timelineHours = new List<ShiftTradeTimeLineHoursViewModel> { new ShiftTradeTimeLineHoursViewModel(), new ShiftTradeTimeLineHoursViewModel() };
			timeLineHoursViewModelFactory.Expect(s => s.CreateTimeLineHours(expectedTimelinePeriod)).Return(timelineHours);

			var result = target.Map(shiftTrade.ShiftTradeSwapDetails.First(), _nameFormatSettings);

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

			var result = target.Map(shiftTrade.ShiftTradeSwapDetails.First(), _nameFormatSettings);

			Assert.That(result.To, Is.Not.Null);
			Assert.That(result.From, Is.Not.Null);
		}

		[Test]
		public void Name_WhenNoScheduleExists_ShouldBeSet()
		{
			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			var shiftTrade = CreateShiftTrade(from, to, null, null);

			var result = target.Map(shiftTrade.ShiftTradeSwapDetails.First(), _nameFormatSettings);

			result.PersonFrom
				.Should().Be.EqualTo(_fromPerson.Name.ToString());
			result.PersonTo
				.Should().Be.EqualTo(_toPerson.Name.ToString());
		}

		[Test]
		public void ShouldMapSwapDate()
		{
			var from = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var to = new DateTime(2001, 1, 2, 0, 0, 0, DateTimeKind.Utc);
			var expected = "2001-01-01T00:00:00.0000000";

			var shiftTrade = CreateShiftTrade(from, to, null, null);

			var result = target.Map(shiftTrade.ShiftTradeSwapDetails.First(), _nameFormatSettings);

			result.Date.Should().Be.EqualTo(expected);
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
			var result = target.Map(scheduleDay);
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
			var result = target.Map(scheduleDay);
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
			var result = target.Map(scheduleDay);

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

			var result = target.Map(scheduleDay);

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

			var result = target.Map(scheduleDay);

			result.ScheduleLayers.First().Payload.Should().Be.EqualTo(activtyName);
		}

		[Test]
		public void ShouldMapDayoffDate()
		{
			var scheduleDay = _scheduleFactory.ScheduleDayStub(new DateTime(2000, 1, 1), _person);
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, new Scenario("d"), new DateOnly(2000, 1, 1), new DayOffTemplate()));
			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub(new IVisualLayer[0]));

			var result = target.Map(scheduleDay);
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

			var result = target.Map(scheduleDay);

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

			var result = target.Map(scheduleDay);

			var expectedValue = layerPeriod2.StartDateTime.Subtract(layerPeriod1.StartDateTime).TotalMinutes;
			result.ScheduleLayers.Last().ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(expectedValue);
		}

		[Test]
		public void ShouldMapDayOff()
		{
			var theDayOff = new DayOffTemplate(new Description("my day off"));
			var personDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_person, new Scenario("scenario"), DateOnly.Today, theDayOff);
			var scheduleDay = _scheduleFactory.ScheduleDayStub(DateTime.Now, _person, SchedulePartView.DayOff, personDayOff);

			_projectionProvider.Expect(p => p.Projection(scheduleDay)).Return(_scheduleFactory.ProjectionStub());

			var result = target.Map(scheduleDay);

			result.ScheduleLayers.Count().Should().Be.EqualTo(1);
			result.DayOffText.Should().Be.EqualTo(theDayOff.Description.Name);
			result.MinutesSinceTimeLineStart.Should().Be.EqualTo(15);

			var dayOffLayer = result.ScheduleLayers.First();
			dayOffLayer.Color.Should().Be.Empty();
			dayOffLayer.ElapsedMinutesSinceShiftStart.Should().Be.EqualTo(0);
			dayOffLayer.LengthInMinutes.Should().Be.EqualTo(TimeSpan.FromHours(9).TotalMinutes);
		}

		[Test]
		public void ShouldUseLoggedOnPersonSchedulePeriodWhenBothDayoff()
		{
			var expactedTime = new DateTime(2019, 1, 31, 0, 0, 0, DateTimeKind.Utc);
			var theDayOff = new DayOffTemplate(new Description("my day off"));
			var personFromDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_fromPerson, new Scenario("scenario"), DateOnly.Today, theDayOff);
			var personToDayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff(_toPerson, new Scenario("scenario"), DateOnly.Today, theDayOff);
			var scheduleDayFrom = _scheduleFactory.ScheduleDayStub(expactedTime, _fromPerson, SchedulePartView.DayOff, personFromDayOff);
			var scheduleDayTo = _scheduleFactory.ScheduleDayStub(expactedTime, _toPerson, SchedulePartView.DayOff, personToDayOff);
			_projectionProvider.Expect(x => x.Projection(scheduleDayFrom)).Return(_scheduleFactory.ProjectionStub());
			_projectionProvider.Expect(x => x.Projection(scheduleDayTo)).Return(_scheduleFactory.ProjectionStub());

			var shiftTradeRequest = CreateShiftTrade(_dateFrom, _dateTo, scheduleDayFrom, scheduleDayTo);
			var result = target.Map(shiftTradeRequest.ShiftTradeSwapDetails.First(), _nameFormatSettings);

			result.TimeLineStartDateTime.Should().Be.EqualTo(expactedTime.AddHours(8));
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

			var result = target.Map(myDay);

			result.HasUnderlyingDayOff.Should().Be.True();
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
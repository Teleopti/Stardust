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
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping
{
	[TestFixture]
	public class WeekScheduleViewModelMappingTest
	{
		private IPeriodSelectionViewModelFactory periodSelectionViewModelFactory;
		private IPeriodViewModelFactory periodViewModelFactory;
		private IHeaderViewModelFactory headerViewModelFactory;
		private IScheduleColorProvider scheduleColorProvider;
		private ILoggedOnUser loggedOnUser;
		private WeekScheduleViewModelMapper modelMapper;

		[SetUp]
		public void Setup()
		{
			periodSelectionViewModelFactory = MockRepository.GenerateMock<IPeriodSelectionViewModelFactory>();
			periodViewModelFactory = MockRepository.GenerateMock<IPeriodViewModelFactory>();
			headerViewModelFactory = MockRepository.GenerateMock<IHeaderViewModelFactory>();
			scheduleColorProvider = MockRepository.GenerateMock<IScheduleColorProvider>();
			loggedOnUser = new FakeLoggedOnUser();
			var userCulture = new FakeUserCulture(CultureInfo.GetCultureInfo("sv-SE"));

			modelMapper = new WeekScheduleViewModelMapper(periodSelectionViewModelFactory, periodViewModelFactory,
				headerViewModelFactory, scheduleColorProvider, loggedOnUser, new Now(), userCulture, new CommonViewModelMapper(),
				new OvertimeAvailabilityViewModelMapper(userCulture));
		}

		[Test]
		public void ShouldMapPeriodSelectionFromLegacyFactory()
		{
			var domainData = new WeekScheduleDomainData {Date = DateOnly.Today};
			var periodSelectionViewModel = new PeriodSelectionViewModel();

			periodSelectionViewModelFactory.Stub(x => x.CreateModel(domainData.Date)).Return(periodSelectionViewModel);
			var result = modelMapper.Map(domainData);

			result.PeriodSelection.Should().Be.SameInstanceAs(periodSelectionViewModel);
		}

		[Test]
		public void ShouldMapPeriodsFromLegacyFactory()
		{
			var minMaxTime = new TimePeriod();
			var domainData = new WeekScheduleDayDomainData{
					MinMaxTime = minMaxTime
				};
			var periodViewModels = new[] { new PeriodViewModel(), };

			periodViewModelFactory.Stub(
				x => x.CreatePeriodViewModels(Arg<IEnumerable<IVisualLayer>>.Is.Anything, Arg<TimePeriod>.Is.Anything,
				                              Arg<DateTime>.Is.Anything, Arg<TimeZoneInfo>.Is.Null)).Return(periodViewModels);
			periodViewModelFactory.Stub(
				x => x.CreateOvertimeAvailabilityPeriodViewModels(null, null,minMaxTime)).Return(new OvertimeAvailabilityPeriodViewModel[]{});
			var result = modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData} });

			result.Days.First().Periods.First().Should().Be.SameInstanceAs(periodViewModels.First());
		}

		[Test]
		public void ShouldMapPeriodsForOvertimeAvailabilityFromLegacyFactory()
		{
			var overtimeAvailability = new OvertimeAvailability(new Person(), DateOnly.Today, new TimeSpan(1, 1, 1), new TimeSpan(2, 2, 2));
			var overtimeAvailabilityYesterday = new OvertimeAvailability(new Person(), DateOnly.Today, new TimeSpan(1, 1, 1), new TimeSpan(2, 2, 2));
			var minMaxTime = new TimePeriod();
			var domainData = new WeekScheduleDayDomainData
				{
					OvertimeAvailability = overtimeAvailability,
					OvertimeAvailabilityYesterday = overtimeAvailabilityYesterday,
					MinMaxTime = minMaxTime
				};
			var periodViewModels = new[] { new OvertimeAvailabilityPeriodViewModel() };
			periodViewModelFactory.Stub(
				x => x.CreatePeriodViewModels(Arg<IEnumerable<IVisualLayer>>.Is.Anything, Arg<TimePeriod>.Is.Anything,
											  Arg<DateTime>.Is.Anything, Arg<TimeZoneInfo>.Is.Null)).Return(new List<PeriodViewModel>());
			periodViewModelFactory.Stub(
				x =>
				x.CreateOvertimeAvailabilityPeriodViewModels(overtimeAvailability, overtimeAvailabilityYesterday, minMaxTime)).Return(periodViewModels);

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().Periods.First().Should().Be.SameInstanceAs(periodViewModels.First());
		}

		[Test]
		public void ShouldMapDate()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today};

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().Date.Should().Be.EqualTo(domainData.Date.ToShortDateString());
		}

		[Test]
		public void ShouldMapFixedDate()
		{
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today };

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().FixedDate.Should().Be.EqualTo(domainData.Date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapDayOfWeekNumber()
		{
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today };

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().DayOfWeekNumber.Should().Be.EqualTo((int)domainData.Date.DayOfWeek);
		}

		[Test]
		public void ShouldMapAvailability()
		{
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today };

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().Availability.Should().Be.EqualTo((bool)domainData.Availability);
		}

		[Test]
		public void ShouldMapStateToday()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today};

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().State.Should().Be.EqualTo(SpecialDateState.Today);
		}

		[Test]
		public void ShouldMapNoSpecialState()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today.AddDays(-2)};

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().State.Should().Be.EqualTo((SpecialDateState) 0);
		}

		[Test]
		public void ShouldMapDayHeaderFromLegacyFactory()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today.AddDays(-2)};
			var headerViewModel = new HeaderViewModel();

			headerViewModelFactory.Stub(x => x.CreateModel(domainData.ScheduleDay)).Return(headerViewModel);

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().Header.Should().Be.EqualTo(headerViewModel);
		}

		[Test]
		public void ShouldMapPublicNote()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today};
			var publicNote = new StubFactory().PublicNoteStub();
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date, SchedulePartView.None, publicNote);
			domainData.ScheduleDay = scheduleDay;

			var result = modelMapper.Map(new WeekScheduleDomainData { Days = new List<WeekScheduleDayDomainData> { domainData } });

			result.Days.First().Note.Message.Should().Be.EqualTo(publicNote.GetScheduleNote(new NoFormatting()));
		}

		[Test]
		public void ShouldMapOvertimeAvailability()
		{
			var overtimeAvailability = new OvertimeAvailability(new Person(), DateOnly.Today, new TimeSpan(1, 1, 1), new TimeSpan(1, 2, 2, 2));
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today, OvertimeAvailability = overtimeAvailability };

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.OvertimeAvailabililty.StartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(overtimeAvailability.StartTime.Value, CultureInfo.GetCultureInfo("sv-SE")));
			result.OvertimeAvailabililty.EndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(overtimeAvailability.EndTime.Value, CultureInfo.GetCultureInfo("sv-SE")));
			result.OvertimeAvailabililty.EndTimeNextDay.Should().Be.EqualTo(true);
			result.OvertimeAvailabililty.HasOvertimeAvailability.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityDefaultValuesForEmpty()
		{
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date, SchedulePartView.None, (IPublicNote)null);
			domainData.ScheduleDay = scheduleDay;

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}})
					.Days.First();

			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTimeNextDay.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityDefaultValuesForDayOff()
		{
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today };
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date, SchedulePartView.DayOff, (IPublicNote)null);
			domainData.ScheduleDay = scheduleDay;

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(8, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(new TimeSpan(17, 0, 0), CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTimeNextDay.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityDefaultValuesIfHasShift()
		{
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today };
	
			var stubs = new StubFactory();
			var person = new Person();
			var personAssignment = new PersonAssignment(person,new Scenario("s"),new DateOnly(2011,5,18));
			var dateTimePeriod = new DateTimePeriod(new DateTime(2011,5,18,6,0,0,DateTimeKind.Utc), new DateTime(2011,5,18,15,0,0,DateTimeKind.Utc));
			personAssignment.AddActivity(new Activity("a") { InWorkTime = true },dateTimePeriod);
			personAssignment.SetShiftCategory(new ShiftCategory("sc"));
			
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011,5,18),SchedulePartView.MainShift,personAssignment);
			domainData.ScheduleDay = scheduleDay;
			domainData.Projection = new VisualLayerCollection(person,new List<IVisualLayer>(), new ProjectionPayloadMerger());

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.OvertimeAvailabililty.DefaultStartTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(dateTimePeriod.TimePeriod(scheduleDay.TimeZone).EndTime, CultureInfo.CurrentCulture));
			result.OvertimeAvailabililty.DefaultEndTime.Should().Be.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(dateTimePeriod.TimePeriod(scheduleDay.TimeZone).EndTime.Add(TimeSpan.FromHours(1)), CultureInfo.CurrentCulture));
		}

		[Test]
		public void ShouldMapTextRequestCount()
		{
			var textRequest = new PersonRequest(new Person(), new TextRequest(new DateTimePeriod()));
			var domainData = new WeekScheduleDayDomainData
			                 	{
			                 		Date = DateOnly.Today,
			                 		PersonRequests = new[] {textRequest, textRequest, new PersonRequest(new Person())}
			                 	};

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.TextRequestCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMapSummaryForDayWithOtherSignificantPart()
		{
			var domainData = new WeekScheduleDayDomainData
			                 	{Date = DateOnly.Today, ScheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date)};

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.Summary.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapSummaryForDayOff()
		{
			var domainData = new WeekScheduleDayDomainData
			                 	{
			                 		Date = DateOnly.Today,
			                 		ScheduleDay =
			                 			new StubFactory().ScheduleDayStub(DateTime.Now.Date, SchedulePartView.DayOff,
																															PersonAssignmentFactory.CreateAssignmentWithDayOff())
			                 	};

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.Summary.Title.Should().Be.EqualTo(domainData.ScheduleDay.PersonAssignment().DayOff().Description.Name);
			result.Summary.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapSummaryForMainShift()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(),new Scenario("s"),new DateOnly(2011,5,18));
			var period = new DateTimePeriod(2011,5,18,7,2011,5,18,16);
			personAssignment.AddActivity(new Activity("a") { InWorkTime = true },period);
			personAssignment.SetShiftCategory(new ShiftCategory("sc"));
		
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011,5,18),SchedulePartView.MainShift,personAssignment);

			var dateTimePeriod = new DateTimePeriod(new DateTime(2011,5,18,6,0,0,DateTimeKind.Utc),
				new DateTime(2011,5,18,15,0,0,DateTimeKind.Utc));

			var projection = stubs.ProjectionStub(dateTimePeriod);
			periodViewModelFactory = new PeriodViewModelFactory(new FakeUserTimeZone(TimeZoneInfo.Utc), new OvertimeAvailabilityViewModelMapper(new FakeUserCulture()));

			var domainData = new WeekScheduleDayDomainData
			{
				Date = new DateOnly(2011,05,18),
				ScheduleDay = scheduleDay,
				Projection = projection,
				MinMaxTime = new TimePeriod(TimeSpan.FromMinutes(15),TimeSpan.FromHours(8))
			};

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.Summary.TimeSpan.Should().Be.EqualTo(period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
			result.Summary.Title.Should().Be.EqualTo(scheduleDay.PersonAssignment().ShiftCategory.Description.Name);
			result.Summary.StyleClassName.Should().Be.EqualTo(scheduleDay.PersonAssignment().ShiftCategory.DisplayColor.ToStyleClass());
			result.Summary.Summary.Should().Be.EqualTo(TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(), CultureInfo.CurrentUICulture));
		}


		[Test]
		public void ShouldNotMapPersonalActivityToSummaryTimespan()
		{
			var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(new Person(),new Scenario("s"),new DateOnly(2011,5,18));
			var period = new DateTimePeriod(2011,5,18,7,2011,5,18,16);
			personAssignment.AddActivity(new Activity("a") { InWorkTime = true },period);
			personAssignment.SetShiftCategory(new ShiftCategory("sc"));
			personAssignment.AddPersonalActivity(new Activity("b") { InWorkTime = true },period.MovePeriod(TimeSpan.FromHours(-2)));

			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011,5,18),SchedulePartView.MainShift,personAssignment);

			var dateTimePeriod = new DateTimePeriod(new DateTime(2011,5,18,6,0,0,DateTimeKind.Utc),
				new DateTime(2011,5,18,15,0,0,DateTimeKind.Utc));
			
			var projection = stubs.ProjectionStub(dateTimePeriod);			
			periodViewModelFactory = new PeriodViewModelFactory(new FakeUserTimeZone(TimeZoneInfo.Utc), new OvertimeAvailabilityViewModelMapper(new FakeUserCulture()));

			var domainData = new WeekScheduleDayDomainData
			{
				Date = new DateOnly(2011,05,18),
				ScheduleDay = scheduleDay,
				Projection = projection,
				MinMaxTime = new TimePeriod(TimeSpan.FromMinutes(15),TimeSpan.FromHours(8))
			};

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.Summary.TimeSpan.Should().Be.EqualTo(period.TimePeriod(scheduleDay.TimeZone).ToShortTimeString());
		}


		[Test]
		public void ShouldMapSummaryForAbsence()
		{
			var stubs = new StubFactory();
			var absenceToDisplay = stubs.PersonAbsenceStub();
			absenceToDisplay.Layer.Payload.Priority = 1;
			var highPriority = stubs.PersonAbsenceStub();
			highPriority.Layer.Payload.Priority= 2;
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now.Date, SchedulePartView.FullDayAbsence, new[] {absenceToDisplay, highPriority});
			var projection = stubs.ProjectionStub();
			var domainData = new WeekScheduleDayDomainData
			                 	{Date = DateOnly.Today, ScheduleDay = scheduleDay, Projection = projection};

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.Summary.Title.Should().Be.EqualTo(absenceToDisplay.Layer.Payload.Description.Name);
			result.Summary.StyleClassName.Should().Be.EqualTo(absenceToDisplay.Layer.Payload.DisplayColor.ToStyleClass());
			result.Summary.Summary.Should().Be.EqualTo(TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(), CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapStyleClassViewModelsFromScheduleColors()
		{
			var domainData = new WeekScheduleDomainData
			                 	{
									Date = DateOnly.Today,
			                 		Days = new WeekScheduleDayDomainData[] {}
			                 	};
			var colors = new[] {Color.Red, Color.Blue};
			scheduleColorProvider.Stub(x => x.GetColors(domainData.ColorSource)).Return(colors);

			var result = modelMapper.Map(domainData);

			result.Styles.Select(s => s.Name)
				.Should().Have.SameValuesAs(Color.Blue.ToStyleClass(), Color.Red.ToStyleClass());
			result.Styles.Select(s => s.ColorHex)
				.Should().Have.SameValuesAs(Color.Blue.ToHtml(), Color.Red.ToHtml());
			result.Styles.Select(s => s.RgbColor)
				.Should().Have.SameValuesAs(Color.Blue.ToCSV(), Color.Red.ToCSV());
		}

		[Test]
		public void ShouldMapStyleClassForAbsenceOnPersonDayOff()
		{
			var stubs = new StubFactory();
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.ContractDayOff, stubs.PersonAbsenceStub());
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today, ScheduleDay = scheduleDay };
			domainData.Projection = new VisualLayerCollection(new Person(), new List<IVisualLayer>(),
				new ProjectionPayloadMerger());

			var result =
				modelMapper.Map(new WeekScheduleDomainData {Days = new List<WeekScheduleDayDomainData> {domainData}}).Days.First();

			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapTextRequestPermission()
		{
			var domainData = new WeekScheduleDomainData
			                 	{
									Date = DateOnly.Today,
									TextRequestPermission = true,
			                 		Days = new WeekScheduleDayDomainData[] {}
			                 	};

			var result = modelMapper.Map(domainData);

			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapTimeLine()
		{
			loggedOnUser.CurrentUser().PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var domainData = new WeekScheduleDomainData
			{
				Date = DateOnly.Today,
				MinMaxTime = new TimePeriod(8, 30, 17, 30)
			};

			var result = modelMapper.Map(domainData);
			result.TimeLine.Count().Should().Be.EqualTo(11);
			result.TimeLine.First().Time.Hours.Should().Be.EqualTo(8);
			result.TimeLine.First().Time.Minutes.Should().Be.EqualTo(30);
			result.TimeLine.First().PositionPercentage.Should().Be.EqualTo(0.0);
			result.TimeLine.ElementAt(1).Time.Hours.Should().Be.EqualTo(9);
			result.TimeLine.ElementAt(1).Time.Minutes.Should().Be.EqualTo(0);
			result.TimeLine.ElementAt(1).PositionPercentage.Should().Be.EqualTo(0.5/(17.5 - 8.5));
		}

		[Test]
		public void ShouldMapTimeLineCulture()
		{
			loggedOnUser.CurrentUser().PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			var domainData = new WeekScheduleDomainData
			{
				Date = DateOnly.Today,
				MinMaxTime = new TimePeriod(8, 30, 17, 30)
			};

			var result = modelMapper.Map(domainData);
			result.TimeLineCulture.Should().Be.EqualTo("sv-SE");
		}

		[Test]
		public void ShouldMapAsmPermission()
		{
			var domainData = new WeekScheduleDomainData()
			{
				Date = DateOnly.Today,
				AsmPermission = true
			};

			var result = modelMapper.Map(domainData);
			result.AsmPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapViewPossibilityPermission()
		{
			var domainData = new WeekScheduleDomainData()
			{
				Date = DateOnly.Today,
				ViewPossibilityPermission = true
			};

			var result = modelMapper.Map(domainData);
			result.ViewPossibilityPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapAbsenceRequestPermission()
		{
			var domainData = new WeekScheduleDomainData
			{
				AbsenceRequestPermission = true,
				Date = DateOnly.Today,
				Days = new WeekScheduleDayDomainData[] { }
			};

			var result = modelMapper.Map(domainData);

			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapOvertimeAvailabilityPermission()
		{
			var domainData = new WeekScheduleDomainData
			{
				Date = DateOnly.Today,
				OvertimeAvailabilityPermission = true,
				Days = new WeekScheduleDayDomainData[] { }
			};

			var result = modelMapper.Map(domainData);

			result.RequestPermission.OvertimeAvailabilityPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsCurrentWeek()
		{
			var domainData = new WeekScheduleDomainData()
			{
				Date = DateOnly.Today,
				IsCurrentWeek = true
			};

			var result = modelMapper.Map(domainData);
			result.IsCurrentWeek.Should().Be.True();
		}

		[Test]
		public void ShouldMapSiteOpenHourIntradayTimePeriod()
		{
			var timePeriod = new TimePeriod(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0));
			var domainData = new WeekScheduleDomainData()
			{
				Date = DateOnly.Today,
				SiteOpenHourIntradayPeriod = timePeriod
			};

			var result = modelMapper.Map(domainData);
			result.SiteOpenHourIntradayPeriod.Equals(timePeriod).Should().Be.True();
		}

		[Test]
		public void ShouldMapDateFormatForUser()
		{
			IPerson person = loggedOnUser.CurrentUser();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			
			var result = modelMapper.Map(new WeekScheduleDomainData
			{
				Date = DateOnly.Today
			});

			var expectedFormat = person.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
			result.DatePickerFormat.Should().Be.EqualTo(expectedFormat);
		}
	}
}
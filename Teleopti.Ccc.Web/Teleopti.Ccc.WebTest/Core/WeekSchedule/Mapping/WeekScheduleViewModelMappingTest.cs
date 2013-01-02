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
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Core.RequestContext;
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
		private IPermissionProvider permissionProvider;
		private ILoggedOnUser loggedOnUser;

		[SetUp]
		public void Setup()
		{
			periodSelectionViewModelFactory = MockRepository.GenerateMock<IPeriodSelectionViewModelFactory>();
			periodViewModelFactory = MockRepository.GenerateMock<IPeriodViewModelFactory>();
			headerViewModelFactory = MockRepository.GenerateMock<IHeaderViewModelFactory>();
			scheduleColorProvider = MockRepository.GenerateMock<IScheduleColorProvider>();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();

			Mapper.Reset();
			Mapper.Initialize(c =>
			                  	{
			                  		c.AddProfile(new WeekScheduleViewModelMappingProfile(
			                  		             	() => Mapper.Engine,
			                  		             	() => periodSelectionViewModelFactory,
			                  		             	() => periodViewModelFactory,
			                  		             	() => headerViewModelFactory,
			                  		             	() => scheduleColorProvider,
			                  		             	() => permissionProvider,
													() => loggedOnUser
 			                  		             	));
			                  		c.AddProfile(new CommonViewModelMappingProfile());
			                  	});
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapPeriodSelectionFromLegacyFactory()
		{
			var domainData = new WeekScheduleDomainData {Date = DateOnly.Today};
			var periodSelectionViewModel = new PeriodSelectionViewModel();

			periodSelectionViewModelFactory.Stub(x => x.CreateModel(domainData.Date)).Return(periodSelectionViewModel);
			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);

			result.PeriodSelection.Should().Be.SameInstanceAs(periodSelectionViewModel);
		}

		[Test]
		public void ShouldMapPeriodsFromLegacyFactory()
		{
			var domainData = new WeekScheduleDayDomainData();
			var periodViewModels = new PeriodViewModel[] {};

			periodViewModelFactory.Stub(
				x =>
				x.CreatePeriodViewModels(Arg<IVisualLayerCollection>.Is.Anything, Arg<TimePeriod>.Is.Anything,
				                         Arg<DateTime>.Is.Anything, Arg<TimeZoneInfo>.Is.Anything)).Return(periodViewModels);

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Periods.Should().Be.SameInstanceAs(periodViewModels);
		}

		[Test]
		public void ShouldMapDate()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today};

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Date.Should().Be.EqualTo(domainData.Date.ToShortDateString());
		}

		[Test]
		public void ShouldMapFixedDate()
		{
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today };

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.FixedDate.Should().Be.EqualTo(domainData.Date.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapStateToday()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today};

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.State.Should().Be.EqualTo(SpecialDateState.Today);
		}

		[Test]
		public void ShouldMapNoSpecialState()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today.AddDays(-2)};

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.State.Should().Be.EqualTo((SpecialDateState) 0);
		}

		[Test]
		public void ShouldMapDayHeaderFromLegacyFactory()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today.AddDays(-2)};
			var headerViewModel = new HeaderViewModel();

			headerViewModelFactory.Stub(x => x.CreateModel(domainData.ScheduleDay)).Return(headerViewModel);

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Header.Should().Be.EqualTo(headerViewModel);
		}

		[Test]
		public void ShouldMapPublicNote()
		{
			var domainData = new WeekScheduleDayDomainData {Date = DateOnly.Today};
			var publicNote = new StubFactory().PublicNoteStub();
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date, SchedulePartView.None, publicNote);
			domainData.ScheduleDay = scheduleDay;

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Note.Message.Should().Be.EqualTo(publicNote.GetScheduleNote(new NoFormatting()));
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

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.TextRequestCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldMapSummaryForDayWithOtherSignificantPart()
		{
			var domainData = new WeekScheduleDayDomainData
			                 	{Date = DateOnly.Today, ScheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date)};

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

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
			                 			                                  new StubFactory().PersonDayOffStub())
			                 	};

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Summary.Title.Should().Be.EqualTo(domainData.ScheduleDay.PersonDayOffCollection().Single().DayOff.Description.Name);
			result.Summary.StyleClassName.Should().Contain(StyleClasses.DayOff);
			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapSummaryForMainShift()
		{
			var stubs = new StubFactory();
			var personAssignment =
				stubs.PersonAssignmentStub(new DateTimePeriod(new DateTime(2011, 5, 18, 6, 0, 0, DateTimeKind.Utc),
				                                              new DateTime(2011, 5, 18, 15, 0, 0, DateTimeKind.Utc)));
			var scheduleDay = stubs.ScheduleDayStub(new DateTime(2011, 5, 18), SchedulePartView.MainShift, personAssignment);
			var projection = stubs.ProjectionStub();
			var domainData = new WeekScheduleDayDomainData
			                 	{Date = DateOnly.Today, ScheduleDay = scheduleDay, Projection = projection};

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Summary.TimeSpan.Should().Be.EqualTo(new TimePeriod(8, 0, 17, 0).ToShortTimeString());
			result.Summary.Title.Should().Be.EqualTo(scheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.Description.Name);
			result.Summary.StyleClassName.Should().Be.EqualTo(scheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.DisplayColor.ToStyleClass());
			result.Summary.Summary.Should().Be.EqualTo(TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(), CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapSummaryForAbsence()
		{
			var stubs = new StubFactory();
			var absenceToDisplay = stubs.PersonAbsenceStub();
			var lowPriorityAbsence = stubs.PersonAbsenceStub();
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now.Date, SchedulePartView.FullDayAbsence, new[] {absenceToDisplay, lowPriorityAbsence});
			var projection = stubs.ProjectionStub();
			var domainData = new WeekScheduleDayDomainData
			                 	{Date = DateOnly.Today, ScheduleDay = scheduleDay, Projection = projection};

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Summary.Title.Should().Be.EqualTo(absenceToDisplay.Layer.Payload.Description.Name);
			result.Summary.StyleClassName.Should().Be.EqualTo(absenceToDisplay.Layer.Payload.DisplayColor.ToStyleClass());
			result.Summary.Summary.Should().Be.EqualTo(TimeHelper.GetLongHourMinuteTimeString(projection.ContractTime(), CultureInfo.CurrentUICulture));
		}

		[Test]
		public void ShouldMapStyleClassViewModelsFromScheduleColors()
		{
			var domainData = new WeekScheduleDomainData
			                 	{
			                 		Days = new WeekScheduleDayDomainData[] {}
			                 	};
			var colors = new[] {Color.Red, Color.Blue};
			scheduleColorProvider.Stub(x => x.GetColors(domainData.ColorSource)).Return(colors);

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);

			result.Styles.Select(s => s.Name)
				.Should().Have.SameValuesAs(new[] { Color.Blue.ToStyleClass(), Color.Red.ToStyleClass() });
			result.Styles.Select(s => s.ColorHex)
				.Should().Have.SameValuesAs(new[] { Color.Blue.ToHtml(), Color.Red.ToHtml() });
			result.Styles.Select(s => s.RgbColor)
				.Should().Have.SameValuesAs(new[] {Color.Blue.ToCSV(), Color.Red.ToCSV()});
		}

		[Test]
		public void ShouldMapStyleClassForAbsenceOnPersonDayOff()
		{
			var stubs = new StubFactory();
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date, SchedulePartView.ContractDayOff, stubs.PersonAbsenceStub());
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today, ScheduleDay = scheduleDay };

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Summary.StyleClassName.Should().Contain(StyleClasses.Striped);
		}

		[Test]
		public void ShouldMapTextRequestPermission()
		{
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.TextRequests)).
				Return(true);
			var domainData = new WeekScheduleDomainData
			                 	{
			                 		Days = new WeekScheduleDayDomainData[] {}
			                 	};

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);

			result.RequestPermission.TextRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapAbsenceRequestPermission()
		{
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb)).
				Return(true);
			var domainData = new WeekScheduleDomainData
			{
				Days = new WeekScheduleDayDomainData[] { }
			};

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);

			result.RequestPermission.AbsenceRequestPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapTimeLine()
		{
			loggedOnUser.Stub(x => x.CurrentUser().PermissionInformation.Culture()).Return(CultureInfo.CurrentCulture);
			var domainData = new WeekScheduleDomainData()
			{
				MinMaxTime = new TimePeriod(8, 30, 17, 30)
			};

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);

			result.TimeLine.Count().Should().Be.EqualTo(11);
			result.TimeLine.First().Time.Should().Be.EqualTo("08:30");
			result.TimeLine.First().PositionPercentage.Should().Be.EqualTo(0.0);
			result.TimeLine.ElementAt(1).Time.Should().Be.EqualTo("09:00");
			result.TimeLine.ElementAt(1).PositionPercentage.Should().Be.EqualTo(0.5/(17.5 - 8.5));
		}

		[Test]
		public void ShouldMapAsmPermission()
		{
			var domainData = new WeekScheduleDomainData()
			{
				AsmPermission = true
			};

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);
			result.AsmPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapIsCurrentWeek()
		{
			var domainData = new WeekScheduleDomainData()
			{
				IsCurrentWeek = true
			};

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);
			result.IsCurrentWeek.Should().Be.True();
		}
	}
}
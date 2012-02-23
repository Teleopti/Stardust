using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection;
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

		[SetUp]
		public void Setup()
		{
			periodSelectionViewModelFactory = MockRepository.GenerateMock<IPeriodSelectionViewModelFactory>();
			periodViewModelFactory = MockRepository.GenerateMock<IPeriodViewModelFactory>();
			headerViewModelFactory = MockRepository.GenerateMock<IHeaderViewModelFactory>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new WeekScheduleViewModelMappingProfile(
			                                    	() => Mapper.Engine,
			                                    	() => periodSelectionViewModelFactory,
			                                    	() => periodViewModelFactory,
			                                    	() => headerViewModelFactory
			                                    	)));
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
			var domainData = new WeekScheduleDayDomainData
			                 	{Date = DateOnly.Today, Projection = MockRepository.GenerateMock<IVisualLayerCollection>()};
			var periodViewModels = new PeriodViewModel[] {};

			periodViewModelFactory.Stub(x => x.CreatePeriodViewModels(domainData.Projection)).Return(periodViewModels);

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

			result.Note.Message.Should().Be.EqualTo(publicNote.ScheduleNote);
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
		public void ShouldMapStyleClassViewModelsFromUniqueProjectionsVisualLayerColors()
		{
			var stubs = new StubFactory();
			var domainData = new WeekScheduleDomainData
			                 	{
			                 		Days = new[]
			                 		       	{
			                 		       		new WeekScheduleDayDomainData {Projection = stubs.ProjectionStub(new[] {stubs.VisualLayerStub(Color.Red)})},
			                 		       		new WeekScheduleDayDomainData {Projection = stubs.ProjectionStub(new[] {stubs.VisualLayerStub(Color.Red), stubs.VisualLayerStub(Color.Blue)})}
			                 		       	}
			                 	};

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);

			result.Styles.Select(s => s.Name)
				.Should().Have.SameValuesAs(new[] {Color.Blue.ToStyleClass(), Color.Red.ToStyleClass()});
			result.Styles.Select(s => s.ColorHex)
				.Should().Have.SameValuesAs(new[] {Color.Blue.ToHtml(), Color.Red.ToHtml()});
		}

		[Test]
		public void ShouldMapStyleClassViewModelsFromUniqueScheduleDayAssignmentMainShiftCategoryColor()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.MainShift, stubs.PersonAssignmentStub(new DateTimePeriod()));
			var domainData = new WeekScheduleDomainData
			                 	{
			                 		Days = new[]
			                 		       	{
			                 		       		new WeekScheduleDayDomainData {ScheduleDay = scheduleDay}
			                 		       	}
			                 	};

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);

			var color = scheduleDay.AssignmentHighZOrder().MainShift.ShiftCategory.DisplayColor;
			result.Styles.Select(s => s.Name)
				.Should().Have.SameValuesAs(new[] {color.ToStyleClass()});
			result.Styles.Select(s => s.ColorHex)
				.Should().Have.SameValuesAs(new[] {color.ToHtml()});
		}

		[Test]
		public void ShouldMapStyleClassViewModelsFromUniqueScheduleDayPersonAbsenceLayerColor()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.FullDayAbsence, new[] {stubs.PersonAbsenceStub()});
			var domainData = new WeekScheduleDomainData
			                 	{
			                 		Days = new[]
			                 		       	{
			                 		       		new WeekScheduleDayDomainData {ScheduleDay = scheduleDay}
			                 		       	}
			                 	};

			var result = Mapper.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData);

			var color = scheduleDay.PersonAbsenceCollection().First().Layer.Payload.DisplayColor;
			result.Styles.Select(s => s.Name)
				.Should().Have.SameValuesAs(new[] {color.ToStyleClass()});
			result.Styles.Select(s => s.ColorHex)
				.Should().Have.SameValuesAs(new[] {color.ToHtml()});
		}

		[Test]
		public void ShouldMapStyleClassForAbsenceOnPersonDayOff()
		{
			var stubs = new StubFactory();
			var absence = stubs.PersonAbsenceStub();
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date, SchedulePartView.FullDayAbsence,
																new StubFactory().PersonDayOffStub(), null, new[] { absence }, null);
			var projection = stubs.ProjectionStub();
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today, ScheduleDay = scheduleDay, Projection = projection };

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Summary.StyleClassName.Should().Contain("striped");
		}

		[Test]
		public void ShouldMapStylClassForAbsenceOnContractDayOff()
		{
			var stubs = new StubFactory();
			var absence = stubs.PersonAbsenceStub();
			var person = MockRepository.GenerateMock<IPerson>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var personContract = MockRepository.GenerateMock<IPersonContract>();
			var contract = MockRepository.GenerateMock<IContract>();
			var contractSchedule = MockRepository.GenerateMock<IContractSchedule>();

			person.Stub(x => x.Period(DateOnly.Today)).Return(personPeriod);
			personPeriod.Stub(x => x.PersonContract).Return(personContract);
			personContract.Stub(x => x.Contract).Return(contract);
			contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
			personContract.Stub(x => x.ContractSchedule).Return(contractSchedule);
			contractSchedule.Stub(x => x.IsWorkday(Arg<DateOnly>.Is.Anything, Arg<DateOnly>.Is.Anything)).Return(false);

			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Now.Date, person, SchedulePartView.FullDayAbsence, null,
																null, new[] { absence }, null);
			var projection = stubs.ProjectionStub();
			var domainData = new WeekScheduleDayDomainData { Date = DateOnly.Today, ScheduleDay = scheduleDay, Projection = projection };

			var result = Mapper.Map<WeekScheduleDayDomainData, DayViewModel>(domainData);

			result.Summary.StyleClassName.Should().Contain("striped");
		}
	}
}
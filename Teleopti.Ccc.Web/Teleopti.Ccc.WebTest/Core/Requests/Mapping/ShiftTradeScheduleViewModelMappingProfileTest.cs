using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Newtonsoft.Json;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
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
		private IShiftTradeTimeLineHoursViewModelFactory _timelineFactory;
		private ILoggedOnUser _loggedOnUser;
		private IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private IMappingEngine _mapper;

		[SetUp]
		public void Setup()
		{
			_shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			_projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_timelineFactory = MockRepository.GenerateMock<IShiftTradeTimeLineHoursViewModelFactory>();
			_possibleShiftTradePersonsProvider = MockRepository.GenerateMock<IPossibleShiftTradePersonsProvider>();
			_mapper = MockRepository.GenerateMock<IMappingEngine>();

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_person = new Person { Name = new Name("John", "Doe") };
			_person.PermissionInformation.SetDefaultTimeZone(timeZone);
			_loggedOnUser.Stub(u => u.CurrentUser()).Return(_person);

			Mapper.Reset();
			Mapper.Initialize(
				c =>
				c.AddProfile(new ShiftTradeScheduleViewModelMappingProfile(_shiftTradeRequestProvider, _projectionProvider,
				                                                           _timelineFactory, _loggedOnUser,
				                                                           _possibleShiftTradePersonsProvider, _mapper)));
		}
		[Test]
		public void ShouldMapMyScheduleFromReadModel()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, LoadOnlyMyTeam = true };
			var readModel = new PersonScheduleDayReadModel();
			var mySchedule = createPersonScheduleViewModel(data.ShiftTradeDate.Date.AddHours(8),
			                                               data.ShiftTradeDate.Date.AddHours(17));
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new List<IPerson>() };
			var possibleTradeScheduleModel = new List<ShiftTradePersonScheduleViewModel>();

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(DateOnly.Today)).Return(readModel);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_mapper.Stub(x => x.Map<IPersonScheduleDayReadModel, ShiftTradePersonScheduleViewModel>(readModel)).Return(mySchedule);
			_mapper.Stub(
				x =>
				x.Map<DatePersons, IEnumerable<ShiftTradePersonScheduleViewModel>>(persons))
				   .Return(possibleTradeScheduleModel);

			var result = Mapper.Map<ShiftTradeScheduleViewModelData, ShiftTradeScheduleViewModel>(data);

			result.MySchedule.Should().Be.SameInstanceAs(mySchedule);
		}

		private ShiftTradePersonScheduleViewModel createPersonScheduleViewModel(DateTime start, DateTime end)
		{
			return new ShiftTradePersonScheduleViewModel
			{
				ScheduleLayers =
					new List<ShiftTradeScheduleLayerViewModel>
							{
								new ShiftTradeScheduleLayerViewModel
									{
										Start = start,
										End = end
									}
							}
			};
		}

		[Test]
		public void ShouldMapIAmNotScheduledFromReadModel()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, LoadOnlyMyTeam = true };
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new List<IPerson>() };
			var possibleTradeScheduleModel = new List<ShiftTradePersonScheduleViewModel>();

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(data.ShiftTradeDate)).Return(null);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_mapper.Stub(
				x =>
				x.Map<DatePersons, IEnumerable<ShiftTradePersonScheduleViewModel>>(persons))
				   .Return(possibleTradeScheduleModel);

			var result = Mapper.Map<ShiftTradeScheduleViewModelData, ShiftTradeScheduleViewModel>(data);

			result.MySchedule.Should().Be.Null();
		}

		[Test]
		public void ShouldMapPossibleTradesFromReadModel()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, LoadOnlyMyTeam = true };
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new[] { new Person() } };
			var possibleTradeScheduleModel = new List<ShiftTradePersonScheduleViewModel>();

			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_mapper.Stub(
				x =>
				x.Map<DatePersons, IEnumerable<ShiftTradePersonScheduleViewModel>>(persons))
				   .Return(possibleTradeScheduleModel);

			var result = Mapper.Map<ShiftTradeScheduleViewModelData, ShiftTradeScheduleViewModel>(data);

			result.PossibleTradeSchedules.Should().Be.SameInstanceAs(possibleTradeScheduleModel);
		}

		[Test]
		public void ShouldMapPossibleScheduleFromPersonAndDate()
		{
			var persons = new List<IPerson> { new Person() };
			var readModels = new[] { new PersonScheduleDayReadModel() };
			var possibleTradeSchedules = new[] { new ShiftTradePersonScheduleViewModel() };

			_shiftTradeRequestProvider.Stub(
				x => x.RetrievePossibleTradeSchedules(DateOnly.Today, persons))
					.Return(readModels);
			_mapper.Stub(
				x => x.Map<IEnumerable<IPersonScheduleDayReadModel>, IEnumerable<ShiftTradePersonScheduleViewModel>>(readModels))
				   .Return(possibleTradeSchedules);

			var result = Mapper.Map<DatePersons, IEnumerable<ShiftTradePersonScheduleViewModel>>(new DatePersons { Date = DateOnly.Today, Persons = persons });

			result.Should().Be.SameInstanceAs(possibleTradeSchedules);
		}

		[Test]
		public void ShouldMapNoPossibleScheduleFromPersonAndDate()
		{
			var result =
				Mapper.Map<DatePersons, IEnumerable<ShiftTradePersonScheduleViewModel>>(new DatePersons
					{
						Date = DateOnly.Today,
						Persons = new List<IPerson>()
					});
			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldMapPersonScheduleFromReadModel()
		{

			var shift = new Shift
			{
				Projection = new List<SimpleLayer>()
			};
			var model = new Model
				{
					FirstName = "Arne", 
					LastName = "Anka", 
					Shift = shift
				};
			var readModel = new PersonScheduleDayReadModel
			{
				PersonId = Guid.NewGuid(),
				ShiftStart = DateTime.Now,
				Model = JsonConvert.SerializeObject(model)
			};
			var layerViewModels = new List<ShiftTradeScheduleLayerViewModel>();

			_mapper.Stub(x => x.Map<IEnumerable<SimpleLayer>, IEnumerable<ShiftTradeScheduleLayerViewModel>>(shift.Projection)).Return(layerViewModels);

			var result = Mapper.Map<IPersonScheduleDayReadModel, ShiftTradePersonScheduleViewModel>(readModel);

			result.PersonId.Should().Be.EqualTo(readModel.PersonId);
			result.StartTimeUtc.Should().Be.EqualTo(readModel.ShiftStart);
			result.ScheduleLayers.Should().Be.SameInstanceAs(layerViewModels);
			result.Name.Should().Be.EqualTo(UserTexts.Resources.MySchedule);
		}

		[Test]
		public void ShouldMapLayerFromReadModelLayer()
		{
			var readModelLayer = new SimpleLayer
				{
					Start = DateTime.Now,
					End = DateTime.Now.AddHours(1),
					Minutes = 60,
					Color = "green",
					Title = "Phone",
					IsAbsenceConfidential = false
				};

			var result = Mapper.Map<SimpleLayer, ShiftTradeScheduleLayerViewModel>(readModelLayer);

			result.Start.Should().Be.EqualTo(readModelLayer.Start);
			result.End.Should().Be.EqualTo(readModelLayer.End);
			result.LengthInMinutes.Should().Be.EqualTo(readModelLayer.Minutes);
			result.Color.Should().Be.EqualTo(readModelLayer.Color);
			result.Title.Should().Be.EqualTo(readModelLayer.Title);
			result.IsAbsenceConfidential.Should().Be.EqualTo(readModelLayer.IsAbsenceConfidential);
		}

		[Test]
		public void ShouldMapTimeLine8To17WhenNoExistingSchedule()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today };
			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(data.ShiftTradeDate)).Return(null);
			var datePersons = new DatePersons {Date = data.ShiftTradeDate, Persons = new List<IPerson>()};
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data))
			                                  .Return(datePersons);

			_mapper.Stub(x => x.Map<DatePersons, IEnumerable<ShiftTradePersonScheduleViewModel>>(datePersons))
			       .Return(new List<ShiftTradePersonScheduleViewModel>());

			var period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(data.ShiftTradeDate.Date.Add(new TimeSpan(7, 45, 0)),
																			  data.ShiftTradeDate.Date.Add(new TimeSpan(17, 15, 0)),
			                                                                  _person.PermissionInformation.DefaultTimeZone());
			var firstTimeLineHour = new ShiftTradeTimeLineHoursViewModel {HourText = string.Empty, LengthInMinutesToDisplay = 15};
			var secondTimeLineHour = new ShiftTradeTimeLineHoursViewModel {HourText = "8", LengthInMinutesToDisplay = 60};
			var lastTimeLineHour = new ShiftTradeTimeLineHoursViewModel {HourText = "17", LengthInMinutesToDisplay = 15};

			_timelineFactory.Stub(x => x.CreateTimeLineHours(period))
			                .Return(new List<ShiftTradeTimeLineHoursViewModel>
				                {
					                firstTimeLineHour,
					                secondTimeLineHour,
					                lastTimeLineHour
				                });

			var result = Mapper.Map<ShiftTradeScheduleViewModelData, ShiftTradeScheduleViewModel>(data);

			result.TimeLineHours.ElementAt(1).HourText.Should().Be.EqualTo("8");
			result.TimeLineHours.ElementAt(1).LengthInMinutesToDisplay.Should().Be.EqualTo(60);
			
			result.TimeLineHours.Last().HourText.Should().Be.EqualTo("17");
			result.TimeLineHours.Last().LengthInMinutesToDisplay.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldMapTimeLineFromSchedules()
		{
			var mySchedule = new ShiftTradePersonScheduleViewModel
				{
					ScheduleLayers =
						new List<ShiftTradeScheduleLayerViewModel>
							{
								new ShiftTradeScheduleLayerViewModel
									{
										Start = new DateTime(2013, 9, 30, 11, 0, 0),
										End = new DateTime(2013, 9, 30, 16, 0, 0)
									}
							}
				};

			var possibleTradeSchedule = new ShiftTradePersonScheduleViewModel
			{
				ScheduleLayers =
					new List<ShiftTradeScheduleLayerViewModel>
							{
								new ShiftTradeScheduleLayerViewModel
									{
										Start = new DateTime(2013, 9, 30, 6, 0, 0),
										End = new DateTime(2013, 9, 30, 13, 0, 0)
									}
							}
			};
			_mapper.Stub(
				x =>
				x.Map<IPersonScheduleDayReadModel, ShiftTradePersonScheduleViewModel>(Arg<IPersonScheduleDayReadModel>.Is.Anything))
			       .Return(mySchedule);

			_mapper.Stub(
				x =>
				x.Map<DatePersons, IEnumerable<ShiftTradePersonScheduleViewModel>>(Arg<DatePersons>.Is.Anything))
				   .Return(new List<ShiftTradePersonScheduleViewModel> { possibleTradeSchedule });

			var earliestStartUtc = TimeZoneHelper.ConvertToUtc(
				possibleTradeSchedule.ScheduleLayers.First().Start.AddMinutes(-15), _person.PermissionInformation.DefaultTimeZone());

			var latestEndUtc = TimeZoneHelper.ConvertToUtc(
				mySchedule.ScheduleLayers.Last().End.AddMinutes(15), _person.PermissionInformation.DefaultTimeZone());

			var period = new DateTimePeriod(earliestStartUtc, latestEndUtc);
			var timeLineHours = new List<ShiftTradeTimeLineHoursViewModel>();
			
			_timelineFactory.Stub(x => x.CreateTimeLineHours(period)).Return(timeLineHours);

			var result = Mapper.Map<ShiftTradeScheduleViewModelData, ShiftTradeScheduleViewModel>(new ShiftTradeScheduleViewModelData());

			result.TimeLineHours.Should().Be.SameInstanceAs(timeLineHours);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeScheduleViewModelMapperTest
	{
		private IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private IShiftTradeAddPersonScheduleViewModelMapper _shiftTradePersonScheduleViewModelMapper;
		private IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;
		private ShiftTradeScheduleViewModelMapper _target;
		private IPersonRequestRepository _personRequestRepository;
		private IScheduleProvider _scheduleProvider;
		private ILoggedOnUser _loggedOnUser;
		private IProjectionProvider _projectionProvider;

		[SetUp]
		public void Setup()
		{
			_shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			_possibleShiftTradePersonsProvider = MockRepository.GenerateMock<IPossibleShiftTradePersonsProvider>();
			_shiftTradePersonScheduleViewModelMapper = MockRepository.GenerateMock<IShiftTradeAddPersonScheduleViewModelMapper>();
			_shiftTradeTimeLineHoursViewModelMapper = MockRepository.GenerateMock<IShiftTradeTimeLineHoursViewModelMapper>();
			_personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			_scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_projectionProvider = new ProjectionProvider();

			_target = new ShiftTradeScheduleViewModelMapper(_shiftTradeRequestProvider, _possibleShiftTradePersonsProvider,
				_shiftTradePersonScheduleViewModelMapper, _shiftTradeTimeLineHoursViewModelMapper, _personRequestRepository,
				_scheduleProvider, _loggedOnUser,
				new ShiftTradeSiteOpenHourFilter(_loggedOnUser, new SiteOpenHoursSpecification(), _projectionProvider), null, null, null, null, null, null, null);
		}

		[Test]
		public void ShouldMapMyScheduleFromReadModel()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId },
				Paging = new Paging {Take = 1}
			};
			var readModel = new PersonScheduleDayReadModel();
			var mySchedule = new ShiftTradeAddPersonScheduleViewModel();
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new List<IPerson>() };

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(DateOnly.Today)).Return(readModel);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(readModel, true, false)).Return(mySchedule);

			var result = _target.Map(data);
			result.MySchedule.Should().Be.SameInstanceAs(mySchedule);
		}
		
		[Test]
		public void ShouldMapIAmNotScheduledFromReadModel()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId },
			};
			var persons = new DatePersons
			{
				Date = data.ShiftTradeDate,
				Persons = new List<IPerson>()
			};
			IPersonScheduleDayReadModel mySchedule = null;

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(data.ShiftTradeDate)).Return(null);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(mySchedule, true, false)).Return(null);

			var result = _target.Map(data);

			result.MySchedule.Should().Be.Null();
		}
		
		[Test]
		public void ShouldMapPossibleTradesFromReadModel()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId },
				Paging = new Paging {Take = 1}
			};
			var persons = new DatePersons
			{
				Date = data.ShiftTradeDate,
				Persons = new[] {new Person()}
			};
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

			stubMySchedule();
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(persons.Date, persons.Persons, data.Paging, null))
				.Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			var result = _target.Map(data);

			result.PossibleTradeSchedules.Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldMapBulletinShiftsWithTimeFilterFromReadModel()
		{
			var myScheduleStart = new DateTime(2012, 8, 28, 12, 0, 0, DateTimeKind.Utc);
			var myScheduleEnd = new DateTime(2012, 8, 28, 16, 0, 0, DateTimeKind.Utc);
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId },
				Paging = new Paging {Take = 1},
				TimeFilter = new TimeFilterInfo()
			};
			var shiftTradePerson = new Person().WithId();
			var persons = new DatePersons
			{
				Date = data.ShiftTradeDate,
				Persons = new[] { shiftTradePerson }
			};
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();
			var myScheduleDayReadModel = new PersonScheduleDayReadModel
			{
				Start = myScheduleStart,
				End = myScheduleEnd
			};
			
			var shiftTradeAddPersonScheduleViewModel = new ShiftTradeAddPersonScheduleViewModel
			{
				StartTimeUtc = myScheduleStart,
				ScheduleLayers = new List<TeamScheduleLayerViewModel>
				{new TeamScheduleLayerViewModel {End = myScheduleEnd}}.ToArray()
			};

			_loggedOnUser.Stub(x => x.CurrentUser()).Return(new Person().WithId());
			var scheduleDay = ScheduleDayFactory.Create(new DateOnly(DateTime.Now), _loggedOnUser.CurrentUser().WithId());
			var scheduleDays =new List<IScheduleDay>{scheduleDay};

			var shiftExchangeOffer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			shiftExchangeOffer.Stub(x => x.IsWantedSchedule(scheduleDay)).Return(true);
			shiftExchangeOffer.Stub(x => x.Person).Return(shiftTradePerson);
			shiftExchangeOffer.Stub(x => x.Date).Return(DateOnly.Today);

			var list = new List<IShiftExchangeOffer>
			{
				shiftExchangeOffer
			};

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(data.ShiftTradeDate)).Return(myScheduleDayReadModel);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(myScheduleDayReadModel, true, false))
				.Return(shiftTradeAddPersonScheduleViewModel);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data,persons.Persons.Select(a=>a.Id.GetValueOrDefault()).ToArray())).Return(persons);

			_scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, persons.Persons))
				.Return(scheduleDays);
			_scheduleProvider.Stub(x => x.GetScheduleForPersons(DateOnly.Today, new List<IPerson> { _loggedOnUser.CurrentUser()}))
				.Return(scheduleDays);

			_shiftTradeRequestProvider.Stub(x => x.RetrieveBulletinTradeSchedules(new List<string>(), data.Paging))
				.Return(scheduleReadModels);

			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			_personRequestRepository.Stub(x => x.FindShiftExchangeOffersForBulletin(new DateOnly(DateTime.Now)))
				.Return(list);

			var result = _target.MapForBulletin(data);

			result.PossibleTradeSchedules.Should().Not.Be.Null();
		}
		
		[Test]
		public void ShouldMapPossibleTradesWithTimeFilterFromReadModel()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid>{teamId},
				Paging = new Paging {Take = 1},
				TimeFilter = new TimeFilterInfo()
			};
			var persons = new DatePersons
			{
				Date = data.ShiftTradeDate,
				Persons = new[] {new Person()}
			};

			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

			stubMySchedule();
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(
				x => x.RetrievePossibleTradeSchedulesWithFilteredTimes(persons.Date, persons.Persons, data.Paging, data.TimeFilter, null))
				.Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			var result = _target.Map(data);

			result.PossibleTradeSchedules.Should().Not.Be.Null();
		}


		[Test]
		public void ShouldMapNoPossibleScheduleFromPersonAndDate()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid>{teamId},
				Paging = new Paging {Take = 1},
			};
			var persons = new DatePersons {Date = data.ShiftTradeDate, Persons = new List<IPerson>()};

			stubMySchedule();
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);

			var result = _target.Map(data);

			result.PossibleTradeSchedules.Should().Be.Empty();
		}

		[Test]
		public void ShouldMapPageCount()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId },
				Paging = new Paging {Skip = 0, Take = 1}
			};
			var persons = new DatePersons {Date = data.ShiftTradeDate, Persons = new[] {new Person()}};
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>
			{
				new ShiftTradeAddPersonScheduleViewModel {Total = 1}
			};
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

			stubMySchedule();
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(persons.Date, persons.Persons, data.Paging, null))
				.Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			var result = _target.Map(data);

			result.PageCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapPossibleTradesForAllPermittedTeamsFromReadModel()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId },
				Paging = new Paging {Take = 1},
				
			};
				
			var persons = new DatePersons {Date = data.ShiftTradeDate, Persons = new[] {new Person()}};
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

			stubMySchedule();
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(persons.Date, persons.Persons, data.Paging, null))
				.Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			var result = _target.Map(data);

			result.PossibleTradeSchedules.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapNoPossibleSchedulesWhenMyScheduleIsFullDayAbsence()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId },
				Paging = new Paging { Take = 1 }
			};
			var readModel = new PersonScheduleDayReadModel();
			var mySchedule = new ShiftTradeAddPersonScheduleViewModel{IsFullDayAbsence = true};
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();
			var teamMateSchedule = new ShiftTradeAddPersonScheduleViewModel();
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new List<IPerson>{PersonFactory.CreatePerson("aa")} };

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(DateOnly.Today)).Return(readModel);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(DateOnly.Today, persons.Persons,data.Paging)).IgnoreArguments().Return(new List<IPersonScheduleDayReadModel>());
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(readModel, true, false)).Return(mySchedule);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(new List<ShiftTradeAddPersonScheduleViewModel>(){teamMateSchedule});

			var result = _target.Map(data);
			result.PossibleTradeSchedules.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldMapWithSiteOpenHourFilter()
		{
			var shiftTradeSiteOpenHourFilter = MockRepository.GenerateMock<IShiftTradeSiteOpenHourFilter>();

			_target = new ShiftTradeScheduleViewModelMapper(_shiftTradeRequestProvider, _possibleShiftTradePersonsProvider,
				_shiftTradePersonScheduleViewModelMapper, _shiftTradeTimeLineHoursViewModelMapper, _personRequestRepository,
				_scheduleProvider, _loggedOnUser, shiftTradeSiteOpenHourFilter, null, null, null, null, null, null, null);

			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid> { teamId },
				Paging = new Paging { Take = 1 }
			};
			var readModel = new PersonScheduleDayReadModel();
			var persons = new DatePersons
			{
				Date = data.ShiftTradeDate,
				Persons = new[] { new Person() }
			};
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();
			var mySchedule = new ShiftTradeAddPersonScheduleViewModel();

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(persons.Date)).Return(readModel);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(persons.Date, persons.Persons, data.Paging, null))
				.Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(readModel, true, false)).Return(mySchedule);

			shiftTradeSiteOpenHourFilter.Stub(x => x.FilterScheduleView(possibleTradeScheduleViewModels, mySchedule, persons))
				.Return(possibleTradeScheduleViewModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			var result = _target.Map(data);
			result.PossibleTradeSchedules.Should().Not.Be.Null();
		}

		private void stubMySchedule()
		{
			var readModel = new PersonScheduleDayReadModel();
			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(DateOnly.Today))
				.Return(readModel);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(readModel, true, false))
				.Return(new ShiftTradeAddPersonScheduleViewModel());
		}

	}
}

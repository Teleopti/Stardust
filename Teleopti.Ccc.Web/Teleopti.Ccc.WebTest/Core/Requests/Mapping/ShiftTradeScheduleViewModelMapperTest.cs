using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

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

			_target = new ShiftTradeScheduleViewModelMapper(_shiftTradeRequestProvider, _possibleShiftTradePersonsProvider,
				_shiftTradePersonScheduleViewModelMapper, _shiftTradeTimeLineHoursViewModelMapper, _personRequestRepository,
				_scheduleProvider, _loggedOnUser);
		}
		
		[Test]
		public void ShouldMapMyScheduleFromReadModel()
		{
			var teamId = Guid.NewGuid();
			var data = new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = DateOnly.Today,
				TeamIdList = new List<Guid>() { teamId },
				Paging = new Paging {Take = 1}
			};
			var readModel = new PersonScheduleDayReadModel();
			var mySchedule = new ShiftTradeAddPersonScheduleViewModel();
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new List<IPerson>() };

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(DateOnly.Today)).Return(readModel);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(readModel, true)).Return(mySchedule);

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
				TeamIdList = new List<Guid>() { teamId },
			};
			var persons = new DatePersons
			{
				Date = data.ShiftTradeDate,
				Persons = new List<IPerson>()
			};
			IPersonScheduleDayReadModel mySchedule = null;

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(data.ShiftTradeDate)).Return(null);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(mySchedule, true)).Return(null);

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
				TeamIdList = new List<Guid>() { teamId },
				Paging = new Paging {Take = 1}
			};
			var persons = new DatePersons
			{
				Date = data.ShiftTradeDate,
				Persons = new[] {new Person()}
			};
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

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
				TeamIdList = new List<Guid>() { teamId },
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
			var myScheduleDayReadModel = new PersonScheduleDayReadModel
			{
				Start = myScheduleStart,
				End = myScheduleEnd
			};

			var person = PersonFactory.CreatePersonWithGuid("", "");
			
			var shiftTradeAddPersonScheduleViewModel = new ShiftTradeAddPersonScheduleViewModel
			{
				StartTimeUtc = myScheduleStart,
				ScheduleLayers = new List<TeamScheduleLayerViewModel>
				{new TeamScheduleLayerViewModel {End = myScheduleEnd}}.ToArray()
			};

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(data.ShiftTradeDate)).Return(myScheduleDayReadModel);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(myScheduleDayReadModel, true))
				.Return(shiftTradeAddPersonScheduleViewModel);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			_scheduleProvider.Stub(x => x.GetScheduleForPersons(new DateOnly(DateTime.Now), persons.Persons))
				.Return(new List<IScheduleDay>());
			_scheduleProvider.Stub(x => x.GetScheduleForPersons(new DateOnly(DateTime.Now), new List<IPerson> {person}))
				.Return(new List<IScheduleDay>());
			_shiftTradeRequestProvider.Stub(x => x.RetrieveBulletinTradeSchedules(new List<string>(), data.Paging))
				.Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

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
				TeamIdList = new List<Guid>(){teamId},
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
				TeamIdList = new List<Guid>(){teamId},
				Paging = new Paging {Take = 1},
			};
			var persons = new DatePersons {Date = data.ShiftTradeDate, Persons = new List<IPerson>()};

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
				TeamIdList = new List<Guid>() { teamId },
				Paging = new Paging {Skip = 0, Take = 1}
			};
			var persons = new DatePersons {Date = data.ShiftTradeDate, Persons = new[] {new Person()}};
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>
			{
				new ShiftTradeAddPersonScheduleViewModel {Total = 1}
			};
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

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
				TeamIdList = new List<Guid>() { teamId },
				Paging = new Paging {Take = 1},
				
			};
				
			var persons = new DatePersons {Date = data.ShiftTradeDate, Persons = new[] {new Person()}};
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(persons.Date, persons.Persons, data.Paging, null))
				.Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			var result = _target.Map(data);

			result.PossibleTradeSchedules.Should().Not.Be.Null();
		}
		
	}
}

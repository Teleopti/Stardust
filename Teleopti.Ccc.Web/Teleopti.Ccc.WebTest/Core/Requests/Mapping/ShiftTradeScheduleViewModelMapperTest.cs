using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
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

		[SetUp]
		public void Setup()
		{
			_shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			_possibleShiftTradePersonsProvider = MockRepository.GenerateMock<IPossibleShiftTradePersonsProvider>();
			_shiftTradePersonScheduleViewModelMapper = MockRepository.GenerateMock<IShiftTradeAddPersonScheduleViewModelMapper>();
			_shiftTradeTimeLineHoursViewModelMapper = MockRepository.GenerateMock<IShiftTradeTimeLineHoursViewModelMapper>();

			_target = new ShiftTradeScheduleViewModelMapper(_shiftTradeRequestProvider, _possibleShiftTradePersonsProvider,
			                                                _shiftTradePersonScheduleViewModelMapper,
			                                                _shiftTradeTimeLineHoursViewModelMapper);
		}

		[Test]
		public void ShouldMapMyScheduleFromReadModel()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, TeamId = Guid.NewGuid(), Paging = new Paging(){Take=1}};
			var readModel = new PersonScheduleDayReadModel();
			var mySchedule = new ShiftTradeAddPersonScheduleViewModel();
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new List<IPerson>() };

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(DateOnly.Today)).Return(readModel);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(readModel)).Return(mySchedule);

			var result = _target.Map(data);
			result.MySchedule.Should().Be.SameInstanceAs(mySchedule);
		}

		[Test]
		public void ShouldMapIAmNotScheduledFromReadModel()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, TeamId = Guid.NewGuid() };
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new List<IPerson>() };
			IPersonScheduleDayReadModel mySchedule = null;

			_shiftTradeRequestProvider.Stub(x => x.RetrieveMySchedule(data.ShiftTradeDate)).Return(null);
			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(mySchedule)).Return(null);

			var result = _target.Map(data);

			result.MySchedule.Should().Be.Null();
		}

		[Test]
		public void ShouldMapPossibleTradesFromReadModel()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, TeamId = Guid.NewGuid(), Paging = new Paging() { Take = 1 } };
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new[] { new Person() } };
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(persons.Date, persons.Persons, data.Paging))
									  .Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);
			
			var result = _target.Map(data);

			result.PossibleTradeSchedules.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapNoPossibleScheduleFromPersonAndDate()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, TeamId = Guid.NewGuid(), Paging = new Paging() { Take = 1 } };
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new List<IPerson>() };

			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);

			var result = _target.Map(data);

			result.PossibleTradeSchedules.Should().Be.Empty();
		}

		[Test]
		public void ShouldMapPageCount()
		{
			var data = new ShiftTradeScheduleViewModelData { ShiftTradeDate = DateOnly.Today, TeamId = Guid.NewGuid(), Paging = new Paging(){Skip = 0, Take = 1} };
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new[] { new Person() } };
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>{new ShiftTradeAddPersonScheduleViewModel(){Total = 1}};
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersons(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(persons.Date, persons.Persons, data.Paging))
									  .Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			var result = _target.Map(data);

			result.PageCount.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldMapPossibleTradesForAllPermittedTeamsFromReadModel()
		{
			var data = new ShiftTradeScheduleViewModelDataForAllTeams() { ShiftTradeDate = DateOnly.Today, TeamIds = new List<Guid>(){Guid.NewGuid()}, Paging = new Paging() { Take = 1 } };
			var persons = new DatePersons { Date = data.ShiftTradeDate, Persons = new[] { new Person() } };
			var possibleTradeScheduleViewModels = new List<ShiftTradeAddPersonScheduleViewModel>();
			var scheduleReadModels = new List<IPersonScheduleDayReadModel>();

			_possibleShiftTradePersonsProvider.Stub(x => x.RetrievePersonsForAllTeams(data)).Return(persons);
			_shiftTradeRequestProvider.Stub(x => x.RetrievePossibleTradeSchedules(persons.Date, persons.Persons, data.Paging))
									  .Return(scheduleReadModels);
			_shiftTradePersonScheduleViewModelMapper.Stub(x => x.Map(scheduleReadModels)).Return(possibleTradeScheduleViewModels);

			var result = _target.Map(data);

			result.PossibleTradeSchedules.Should().Not.Be.Null();
		}
	}
}

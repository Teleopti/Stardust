using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Newtonsoft.Json;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeAddPersonScheduleViewModelMapperTest
	{
		private IPersonNameProvider _personNameProvider;
		private IPersonRepository _personRepo;

		[SetUp]
		public void Setup()
		{
			var nameFormatSettingsPersisterAndProvider = MockRepository.GenerateMock<ISettingsPersisterAndProvider<NameFormatSettings>>();
			nameFormatSettingsPersisterAndProvider.Stub(x => x.Get()).Return(new NameFormatSettings() { NameFormatId = 0 });
			_personNameProvider = new PersonNameProvider(nameFormatSettingsPersisterAndProvider);
			_personRepo = MockRepository.GenerateMock<IPersonRepository>();
		}

		[Test]
		public void ShouldMapPersonScheduleFromReadModel()
		{
			var shift = new Shift
			{
				Projection = new List<SimpleLayer> {new SimpleLayer(), new SimpleLayer()}
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
				Start = DateTime.Now,
				Model = JsonConvert.SerializeObject(model),
				FirstName = "Arne",
				LastName = "Anka"
			};
			var layerViewModels = new ShiftTradeAddScheduleLayerViewModel[] {};
			var layerMapper = MockRepository.GenerateStrictMock<IShiftTradeAddScheduleLayerViewModelMapper>();
			// dont know why I have to ignore arguments here but...
			layerMapper.Expect(x => x.Map(shift.Projection)).Return(layerViewModels).IgnoreArguments().Repeat.Twice();

			var target = new ShiftTradeAddPersonScheduleViewModelMapper(layerMapper, _personNameProvider, _personRepo);
			var result = target.Map(readModel);

			result.PersonId.Should().Be.EqualTo(readModel.PersonId);
			result.StartTimeUtc.Should().Be.EqualTo(readModel.Start);
			result.ScheduleLayers.Should().Be.SameInstanceAs(layerViewModels);
			result.Name.Should().Be.EqualTo(string.Format(CultureInfo.InvariantCulture, "{0} {1}", model.FirstName, model.LastName));
		}

		[Test]
		public void ShouldMapPersonSchedulesFromReadModels()
		{

			var shift = new Shift
			{
				Projection = new List<SimpleLayer> { new SimpleLayer() }
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
				Start = DateTime.Now,
				Model = JsonConvert.SerializeObject(model),
				Total = 2
			};
			var layerViewModels = new ShiftTradeAddScheduleLayerViewModel[]{};
			var layerMapper = MockRepository.GenerateMock<IShiftTradeAddScheduleLayerViewModelMapper>();

			layerMapper.Stub(x => x.Map(shift.Projection)).Return(layerViewModels);

			var target = new ShiftTradeAddPersonScheduleViewModelMapper(layerMapper, _personNameProvider, _personRepo);
			var result = target.Map(new[] { readModel, readModel });

			result.Count.Should().Be.EqualTo(readModel.Total);
		}

		[Test]
		public void ShouldMapMinStartAndLastPageFromReadModel()
		{
			var shift = new Shift
			{
				Projection = new List<SimpleLayer> { new SimpleLayer()}
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
				Start = DateTime.Now,
				Model = JsonConvert.SerializeObject(model),
				IsLastPage = false,
				MinStart = new DateTime(2013,11,28,7,0,0)
			};
			var layerViewModels = new ShiftTradeAddScheduleLayerViewModel[] {};
			var layerMapper = MockRepository.GenerateMock<IShiftTradeAddScheduleLayerViewModelMapper>();

			layerMapper.Stub(x => x.Map(shift.Projection)).Return(layerViewModels);

			var target = new ShiftTradeAddPersonScheduleViewModelMapper(layerMapper, _personNameProvider, _personRepo);

			var result = target.Map(readModel);

			result.MinStart.Should().Be.EqualTo(new DateTime(2013, 11, 28, 7, 0, 0));
		}

		[Test]
		public void ShouldMapOfferId()
		{
			var shift = new Shift
			{
				Projection = new List<SimpleLayer> { new SimpleLayer() }
			};
			var model = new Model
			{
				FirstName = "Arne",
				LastName = "Anka",
				Shift = shift
			};
			var expectedId = Guid.Empty;
			var readModel = new PersonScheduleDayReadModel
			{
				PersonId = Guid.NewGuid(),
				Start = DateTime.Now,
				Model = JsonConvert.SerializeObject(model),
				IsLastPage = false,
				MinStart = new DateTime(2013, 11, 28, 7, 0, 0),
				ShiftExchangeOffer = expectedId
			};
			var layerViewModels = new ShiftTradeAddScheduleLayerViewModel[] {};
			var layerMapper = MockRepository.GenerateMock<IShiftTradeAddScheduleLayerViewModelMapper>();

			layerMapper.Stub(x => x.Map(shift.Projection)).Return(layerViewModels);

			var target = new ShiftTradeAddPersonScheduleViewModelMapper(layerMapper, _personNameProvider, _personRepo);

			var result = target.Map(readModel);
			result.ShiftExchangeOfferId.Should().Be.EqualTo(expectedId);
		}
	}
}

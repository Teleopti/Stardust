using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Newtonsoft.Json;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeAddPersonScheduleViewModelMapperTest
	{
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
				Model = JsonConvert.SerializeObject(model)
			};
			var layerViewModels = new List<ShiftTradeAddScheduleLayerViewModel>();
			var layerMapper = MockRepository.GenerateStrictMock<IShiftTradeAddScheduleLayerViewModelMapper>();
			// dont know why I have to ignore arguments here but...
			layerMapper.Expect(x => x.Map(shift.Projection)).Return(layerViewModels).IgnoreArguments().Repeat.Twice();
			
			var target = new ShiftTradeAddPersonScheduleViewModelMapper(layerMapper);
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
				Model = JsonConvert.SerializeObject(model)
			};
			var layerViewModels = new List<ShiftTradeAddScheduleLayerViewModel>();
			var layerMapper = MockRepository.GenerateMock<IShiftTradeAddScheduleLayerViewModelMapper>();

			layerMapper.Stub(x => x.Map(shift.Projection)).Return(layerViewModels);

			var target = new ShiftTradeAddPersonScheduleViewModelMapper(layerMapper);
			var result = target.Map(new[] { readModel, readModel });

			result.Count.Should().Be.EqualTo(2);
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
			var layerViewModels = new List<ShiftTradeAddScheduleLayerViewModel>();
			var layerMapper = MockRepository.GenerateMock<IShiftTradeAddScheduleLayerViewModelMapper>();

			layerMapper.Stub(x => x.Map(shift.Projection)).Return(layerViewModels);

			var target = new ShiftTradeAddPersonScheduleViewModelMapper(layerMapper);

			var result = target.Map(readModel);

			result.MinStart.Should().Be.EqualTo(new DateTime(2013, 11, 28, 7, 0, 0));
		}
	}
}

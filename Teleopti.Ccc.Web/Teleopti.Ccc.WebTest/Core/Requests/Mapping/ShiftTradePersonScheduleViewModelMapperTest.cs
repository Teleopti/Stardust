using System;
using System.Collections.Generic;
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
	public class ShiftTradePersonScheduleViewModelMapperTest
	{
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
				Start = DateTime.Now,
				Model = JsonConvert.SerializeObject(model)
			};
			var layerViewModels = new List<ShiftTradeScheduleLayerViewModel>();
			var layerMapper = MockRepository.GenerateMock<IShiftTradeScheduleLayerViewModelMapper>();

			layerMapper.Stub(x => x.Map(shift.Projection)).Return(layerViewModels);
			
			var target = new ShiftTradePersonScheduleViewModelMapper(layerMapper);
			var result = target.Map(readModel);

			result.PersonId.Should().Be.EqualTo(readModel.PersonId);
			result.StartTimeUtc.Should().Be.EqualTo(readModel.Start);
			result.ScheduleLayers.Should().Be.SameInstanceAs(layerViewModels);
			result.Name.Should().Be.EqualTo(UserTexts.Resources.MySchedule);
		}

		[Test]
		public void ShouldMapPersonSchedulesFromReadModels()
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
				Start = DateTime.Now,
				Model = JsonConvert.SerializeObject(model)
			};
			var layerViewModels = new List<ShiftTradeScheduleLayerViewModel>();
			var layerMapper = MockRepository.GenerateMock<IShiftTradeScheduleLayerViewModelMapper>();

			layerMapper.Stub(x => x.Map(shift.Projection)).Return(layerViewModels);

			var target = new ShiftTradePersonScheduleViewModelMapper(layerMapper);
			var result = target.Map(new[] { readModel, readModel });

			result.Count.Should().Be.EqualTo(2);
		}
	}
}

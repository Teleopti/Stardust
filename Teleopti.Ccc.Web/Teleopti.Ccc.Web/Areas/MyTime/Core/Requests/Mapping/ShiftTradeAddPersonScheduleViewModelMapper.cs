using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeAddPersonScheduleViewModelMapper : IShiftTradeAddPersonScheduleViewModelMapper
	{
		private readonly IShiftTradeAddScheduleLayerViewModelMapper _layerMapper;

		public ShiftTradeAddPersonScheduleViewModelMapper(IShiftTradeAddScheduleLayerViewModelMapper layerMapper)
		{
			_layerMapper = layerMapper;
		}

		public ShiftTradeAddPersonScheduleViewModel Map(IPersonScheduleDayReadModel scheduleReadModel)
		{
			if (scheduleReadModel == null || scheduleReadModel.Start == null)
				return null;

			var shiftReadModel = JsonConvert.DeserializeObject<Model>(scheduleReadModel.Model);
			if(shiftReadModel.Shift.Projection.Count == 0)
				return null;
			return new ShiftTradeAddPersonScheduleViewModel
			{
				PersonId = scheduleReadModel.PersonId,
				StartTimeUtc = scheduleReadModel.Start.Value,
				Name = string.Format(CultureInfo.InvariantCulture, "{0} {1}", shiftReadModel.FirstName, shiftReadModel.LastName),
				ScheduleLayers = _layerMapper.Map(shiftReadModel.Shift.Projection),
				MinStart = scheduleReadModel.MinStart,
				IsLastPage = scheduleReadModel.IsLastPage
			};
		}

		public IList<ShiftTradeAddPersonScheduleViewModel> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels)
		{
			return scheduleReadModels.Select(Map).Where(s => s != null).ToList();
		}
	}
}
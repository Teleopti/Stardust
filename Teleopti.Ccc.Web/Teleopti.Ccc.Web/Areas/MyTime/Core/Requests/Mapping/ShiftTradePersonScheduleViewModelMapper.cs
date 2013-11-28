using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePersonScheduleViewModelMapper : IShiftTradePersonScheduleViewModelMapper
	{
		private readonly IShiftTradeScheduleLayerViewModelMapper _layerMapper;

		public ShiftTradePersonScheduleViewModelMapper(IShiftTradeScheduleLayerViewModelMapper layerMapper)
		{
			_layerMapper = layerMapper;
		}

		public ShiftTradePersonScheduleViewModel Map(IPersonScheduleDayReadModel scheduleReadModel)
		{
			if (scheduleReadModel == null || scheduleReadModel.Start == null)
				return null;

			var shiftReadModel = JsonConvert.DeserializeObject<Model>(scheduleReadModel.Model);
			return new ShiftTradePersonScheduleViewModel
			{
				PersonId = scheduleReadModel.PersonId,
				StartTimeUtc = scheduleReadModel.Start.Value,
				Name = string.Format(CultureInfo.InvariantCulture, "{0} {1}", shiftReadModel.FirstName, shiftReadModel.LastName),
				ScheduleLayers = _layerMapper.Map(shiftReadModel.Shift.Projection),
				MinStart = scheduleReadModel.MinStart
			};
		}

		public IList<ShiftTradePersonScheduleViewModel> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels)
		{
			return scheduleReadModels.Select(Map).ToList();
		}
	}
}
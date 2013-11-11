using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeScheduleLayerViewModelMapper : IShiftTradeScheduleLayerViewModelMapper
	{
		public IEnumerable<ShiftTradeScheduleLayerViewModel> Map(IEnumerable<SimpleLayer> sourceLayers)
		{
			return sourceLayers.Select(mapLayer);
		}

		private static ShiftTradeScheduleLayerViewModel mapLayer(SimpleLayer sourceLayer)
		{
			return new ShiftTradeScheduleLayerViewModel
				{
					Start = sourceLayer.Start,
					End = sourceLayer.End,
					LengthInMinutes = sourceLayer.Minutes,
					Color = sourceLayer.Color,
					Title = sourceLayer.Description,
					IsAbsenceConfidential = sourceLayer.IsAbsenceConfidential
				};
		}
	}
}
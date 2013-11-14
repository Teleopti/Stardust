using System.Collections.Generic;
using System.Globalization;
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
			var expectedTime = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", 
												sourceLayer.Start.ToShortTimeString(),
												sourceLayer.End.ToShortTimeString());
			return new ShiftTradeScheduleLayerViewModel
				{
					Start = sourceLayer.Start,
					End = sourceLayer.End,
					LengthInMinutes = sourceLayer.Minutes,
					Color = sourceLayer.Color,
					TitleHeader = sourceLayer.Description,
					TitleTime = expectedTime,
					IsAbsenceConfidential = sourceLayer.IsAbsenceConfidential
				};
		}
	}
}
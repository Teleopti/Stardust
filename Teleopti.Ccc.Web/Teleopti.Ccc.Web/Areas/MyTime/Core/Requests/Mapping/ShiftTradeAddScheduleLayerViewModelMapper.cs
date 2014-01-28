using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeAddScheduleLayerViewModelMapper : IShiftTradeAddScheduleLayerViewModelMapper
	{
		private readonly IUserTimeZone _userTimeZone;

		public ShiftTradeAddScheduleLayerViewModelMapper(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public IEnumerable<ShiftTradeAddScheduleLayerViewModel> Map(IEnumerable<SimpleLayer> sourceLayers)
		{
			return sourceLayers.Select(mapLayer);
		}

		private ShiftTradeAddScheduleLayerViewModel mapLayer(SimpleLayer sourceLayer)
		{
			var timeZone = _userTimeZone.TimeZone();
			var start = TimeZoneHelper.ConvertFromUtc(sourceLayer.Start, timeZone);
			var end = TimeZoneHelper.ConvertFromUtc(sourceLayer.End, timeZone);

			var expectedTime = string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
												start.ToShortTimeString(),
												end.ToShortTimeString());
			return new ShiftTradeAddScheduleLayerViewModel
				{
					Start = start,
					End = end,
					LengthInMinutes = sourceLayer.Minutes,
					Color = sourceLayer.Color,
					TitleHeader = sourceLayer.Description,
					TitleTime = expectedTime,
					IsAbsenceConfidential = sourceLayer.IsAbsenceConfidential
				};
		}
	}
}
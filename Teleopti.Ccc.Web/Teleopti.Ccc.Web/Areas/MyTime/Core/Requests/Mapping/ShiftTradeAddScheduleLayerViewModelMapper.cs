using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
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

		public IEnumerable<ShiftTradeAddScheduleLayerViewModel> Map(IEnumerable<SimpleLayer> sourceLayers, bool isMySchedule = false)
		{
			return sourceLayers.Select(layer => mapLayer(layer, isMySchedule));
		}

		private ShiftTradeAddScheduleLayerViewModel mapLayer(SimpleLayer sourceLayer, bool isMySchedule = false)
		{
			var timeZone = _userTimeZone.TimeZone();
			var start = TimeZoneHelper.ConvertFromUtc(sourceLayer.Start, timeZone);
			var end = TimeZoneHelper.ConvertFromUtc(sourceLayer.End, timeZone);

			var expectedTime = string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
												start.ToShortTimeString(),
												end.ToShortTimeString());
			string color = sourceLayer.Color;
			string titleHeader = sourceLayer.Description;
			if (!isMySchedule && sourceLayer.IsAbsenceConfidential)
			{
				color = ConfidentialPayloadValues.DisplayColorHex;
				titleHeader = ConfidentialPayloadValues.Description.Name;
			}
			return new ShiftTradeAddScheduleLayerViewModel
				{
					Start = start,
					End = end,
					LengthInMinutes = sourceLayer.Minutes,
					Color = color,
					TitleHeader = titleHeader,
					TitleTime = expectedTime,
					IsAbsenceConfidential = sourceLayer.IsAbsenceConfidential
				};
		}
	}
}
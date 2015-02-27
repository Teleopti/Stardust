using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class LayerViewModelReworkedMapper : ILayerViewModelReworkedMapper
	{
		private readonly IUserTimeZone _userTimeZone;

		public LayerViewModelReworkedMapper(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}
			
		public IEnumerable<LayerViewModelReworked> Map(IEnumerable<SimpleLayer> sourceLayers)
		{
			return sourceLayers.Select(Map);
		}

		public LayerViewModelReworked Map(SimpleLayer sourceLayer)
		{
			var timeZone = _userTimeZone.TimeZone();
			var start = TimeZoneHelper.ConvertFromUtc(sourceLayer.Start, timeZone);
			var end = TimeZoneHelper.ConvertFromUtc(sourceLayer.End, timeZone);

			var expectedTime = string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
												start.ToShortTimeString(),
												end.ToShortTimeString());
			var color = sourceLayer.Color;
			var titleHeader = sourceLayer.Description;
			if (sourceLayer.IsAbsenceConfidential)
			{
				color = ConfidentialPayloadValues.DisplayColorHex;
				titleHeader = ConfidentialPayloadValues.Description.Name;
			}
			return new LayerViewModelReworked
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
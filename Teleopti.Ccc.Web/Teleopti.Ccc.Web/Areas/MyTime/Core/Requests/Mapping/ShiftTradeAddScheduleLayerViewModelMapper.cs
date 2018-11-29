using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeAddScheduleLayerViewModelMapper : IShiftTradeAddScheduleLayerViewModelMapper
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly IPermissionProvider _permissionProvider;

		public ShiftTradeAddScheduleLayerViewModelMapper(IUserTimeZone userTimeZone, IPermissionProvider permissionProvider)
		{
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
		}

		public TeamScheduleLayerViewModel[] Map(IEnumerable<SimpleLayer> sourceLayers, bool isMySchedule = false)
		{
			return sourceLayers.Select(layer => mapLayer(layer, isMySchedule)).ToArray();
		}

		public TeamScheduleLayerViewModel[] Map(IEnumerable<SimpleLayer> sourceLayers, IEnumerable<OvertimeShiftLayer> overtimeActivities, bool isMySchedule = false)
		{
			return sourceLayers.Select(layer => mapLayer(layer, overtimeActivities, isMySchedule)).ToArray();
		}

		private TeamScheduleLayerViewModel mapLayer(SimpleLayer sourceLayer, IEnumerable<OvertimeShiftLayer> overtimeActivities, bool isMySchedule = false)
		{
			var viewModel = mapLayer(sourceLayer, isMySchedule);
			var layerPeriod = new DateTimePeriod(sourceLayer.Start, sourceLayer.End);
			viewModel.IsOvertime = overtimeActivities != null && 
								   overtimeActivities.Any(overtime => layerPeriod.Intersect(overtime.Period));
			
			return viewModel;
		}

		private TeamScheduleLayerViewModel mapLayer(SimpleLayer sourceLayer, bool isMySchedule = false)
		{
			var timeZone = _userTimeZone.TimeZone();
			var start = TimeZoneHelper.ConvertFromUtc(sourceLayer.Start, timeZone);
			var end = TimeZoneHelper.ConvertFromUtc(sourceLayer.End, timeZone);

			var expectedTime = string.Format(CultureInfo.CurrentCulture, "{0} - {1}",
												start.ToShortTimeString(),
												end.ToShortTimeString());
			string color = sourceLayer.Color;
			string titleHeader = sourceLayer.Description;
			if (!isMySchedule && sourceLayer.IsAbsenceConfidential && !_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential))
			{
				color = ConfidentialPayloadValues.DisplayColorHex;
				titleHeader = ConfidentialPayloadValues.Description.Name;
			}
			return new TeamScheduleLayerViewModel
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
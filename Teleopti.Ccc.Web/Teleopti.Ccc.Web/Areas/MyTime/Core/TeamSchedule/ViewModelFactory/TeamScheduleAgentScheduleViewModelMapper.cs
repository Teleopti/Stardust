using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleAgentScheduleViewModelMapper
	{
		
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public TeamScheduleAgentScheduleViewModelMapper(IPermissionProvider permissionProvider, ILoggedOnUser loggedOnUser)
		{
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
		}

		public IList<TeamScheduleAgentScheduleViewModel> Map(IEnumerable<AgentInTeamScheduleViewModel> agentInTeamScheduleViewModels, DateTimePeriod schedulePeriod)
		{
			var timeZoneInfo = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var startTime = schedulePeriod.StartDateTimeLocal(timeZoneInfo);
			var endTime = schedulePeriod.EndDateTimeLocal(timeZoneInfo);
			var result = new List<TeamScheduleAgentScheduleViewModel>();
			foreach (var agentInTeamScheduleViewModel in agentInTeamScheduleViewModels)
			{
				var personId = agentInTeamScheduleViewModel.PersonId;

				result.Add(new TeamScheduleAgentScheduleViewModel
				{
					Periods = buildPeriods(agentInTeamScheduleViewModel, startTime, endTime,
						personId == _loggedOnUser.CurrentUser().Id),
					Name = agentInTeamScheduleViewModel.Name,
					IsDayOff = agentInTeamScheduleViewModel.IsDayOff,
					DayOffName = agentInTeamScheduleViewModel.DayOffName,
					IsNotScheduled = agentInTeamScheduleViewModel.IsNotScheduled,
					ShiftCategory = agentInTeamScheduleViewModel.ShiftCategory
				});
			}
			return result;
		}

		public TeamScheduleAgentScheduleViewModel Map(
			AgentInTeamScheduleViewModel agentInTeamScheduleViewModel, DateTimePeriod schedulePeriod, bool isMySchedule = false)
		{
			var timeZoneInfo = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var startTime = schedulePeriod.StartDateTimeLocal(timeZoneInfo);
			var endTime = schedulePeriod.EndDateTimeLocal(timeZoneInfo);
			
			List<TeamScheduleAgentScheduleLayerViewModel> periods = buildPeriods(agentInTeamScheduleViewModel, startTime, endTime, isMySchedule);
			return new TeamScheduleAgentScheduleViewModel
			{
				Periods = periods,
				Name = agentInTeamScheduleViewModel.Name,
				IsDayOff = agentInTeamScheduleViewModel.IsDayOff,
				DayOffName = agentInTeamScheduleViewModel.DayOffName,
				IsNotScheduled = agentInTeamScheduleViewModel.IsNotScheduled,
				ShiftCategory = agentInTeamScheduleViewModel.ShiftCategory,
				BelongsToDate = schedulePeriod.StartDateTimeLocal(_loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone())
			};
		}

		private List<TeamScheduleAgentScheduleLayerViewModel> buildPeriods(AgentInTeamScheduleViewModel agentInTeamScheduleViewModel, DateTime startTime, DateTime endTime, bool isMySchedule = false)
		{
			var periods = new List<TeamScheduleAgentScheduleLayerViewModel>();
			var layers = agentInTeamScheduleViewModel.ScheduleLayers;
			if (layers != null && layers.Any())
			{
				var diff = (decimal)(endTime - startTime).Ticks;
				foreach (var layer in layers)
				{

					string color = layer.Color;
					string title = layer.TitleHeader;
					if (!isMySchedule && layer.IsAbsenceConfidential && !_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential))
					{
						color = ConfidentialPayloadValues.DisplayColorHex;
						title = ConfidentialPayloadValues.Description.Name;
					}

				    var scheduleLayerViewModel = new TeamScheduleAgentScheduleLayerViewModel
					{
						Color = color,
						Title = title,
						StartTime = layer.Start,
						EndTime = layer.End,
						IsOvertime = layer.IsOvertime,
						Meeting = layer.Meeting,
						StartPositionPercentage = calculatePosition(layer.Start, startTime, diff),
						EndPositionPercentage = calculatePosition(layer.End, startTime, diff),
						TimeSpan = TimeHelper.TimeOfDayFromTimeSpan(layer.Start.TimeOfDay, CultureInfo.CurrentCulture) + " - " + TimeHelper.TimeOfDayFromTimeSpan(layer.End.TimeOfDay, CultureInfo.CurrentCulture)
					};
					periods.Add(scheduleLayerViewModel);
				}
			}

			return periods;
		}

		private static decimal calculatePosition(DateTime time, DateTime timeLineStartTime, decimal diff)
		{
			return Math.Round((time - timeLineStartTime).Ticks / diff, 4);
		}
	}
}
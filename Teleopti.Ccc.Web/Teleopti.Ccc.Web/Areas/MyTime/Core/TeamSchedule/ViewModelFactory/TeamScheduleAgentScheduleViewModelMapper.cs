using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleAgentScheduleViewModelMapper
	{
		public IList<TeamScheduleAgentScheduleViewModel> Map(IEnumerable<AgentInTeamScheduleViewModel> agentInTeamScheduleViewModels, DateTimePeriod schedulePeriod, TimeZoneInfo timeZoneInfo)
		{
			var startTime = schedulePeriod.StartDateTimeLocal(timeZoneInfo);
			var endTime = schedulePeriod.EndDateTimeLocal(timeZoneInfo);
			var result = new List<TeamScheduleAgentScheduleViewModel>();
			foreach (var agentInTeamScheduleViewModel in agentInTeamScheduleViewModels)
			{
				result.Add(new TeamScheduleAgentScheduleViewModel
				{
					Periods = buildPeriods(agentInTeamScheduleViewModel, startTime, endTime),
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
			AgentInTeamScheduleViewModel agentInTeamScheduleViewModel, DateTimePeriod schedulePeriod,
			TimeZoneInfo timeZoneInfo)
		{
			var startTime = schedulePeriod.StartDateTimeLocal(timeZoneInfo);
			var endTime = schedulePeriod.EndDateTimeLocal(timeZoneInfo);
			
			List<TeamScheduleAgentScheduleLayerViewModel> periods = buildPeriods(agentInTeamScheduleViewModel, startTime, endTime);
			return new TeamScheduleAgentScheduleViewModel
			{
				Periods = periods,
				Name = agentInTeamScheduleViewModel.Name,
				IsDayOff = agentInTeamScheduleViewModel.IsDayOff,
				DayOffName = agentInTeamScheduleViewModel.DayOffName,
				IsNotScheduled = agentInTeamScheduleViewModel.IsNotScheduled,
				ShiftCategory = agentInTeamScheduleViewModel.ShiftCategory
			};
		}

		private static List<TeamScheduleAgentScheduleLayerViewModel> buildPeriods(AgentInTeamScheduleViewModel agentInTeamScheduleViewModel, DateTime startTime, DateTime endTime)
		{
			var periods = new List<TeamScheduleAgentScheduleLayerViewModel>();
			var layers = agentInTeamScheduleViewModel.ScheduleLayers;
			if (layers != null && layers.Any())
			{
				var diff = (decimal)(endTime - startTime).Ticks;
				foreach (var layer in layers)
				{
					var scheduleLayerViewModel = new TeamScheduleAgentScheduleLayerViewModel
					{
						Color = layer.Color,
						Title = layer.TitleHeader,
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
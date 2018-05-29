using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleAgentScheduleViewModelMapper
	{
		public IList<TeamScheduleAgentScheduleViewModel> Map(IEnumerable<AgentInTeamScheduleViewModel> agentInTeamScheduleViewModels,DateTime timeLineStartTime, DateTime timeLineEndTime)
		{
			var result = new List<TeamScheduleAgentScheduleViewModel>();
			foreach (var agentInTeamScheduleViewModel in agentInTeamScheduleViewModels)
			{
				var periods = new List<TeamScheduleAgentScheduleLayerViewModel>();
				var layers = agentInTeamScheduleViewModel.ScheduleLayers;
				var diff = (decimal)(timeLineEndTime - timeLineStartTime).Ticks;
				foreach (var layer in layers)
				{
					var scheduleLayerViewModel = new TeamScheduleAgentScheduleLayerViewModel
					{
						Color = layer.Color,
						Title = layer.TitleHeader,
						StartTime = layer.Start,
						EndTime = layer.End,
						IsOvertime = layer.IsOvertime,
						StartPositionPercentage = calculatePosition(layer.Start,timeLineStartTime, diff),
						EndPositionPercentage = calculatePosition(layer.End , timeLineStartTime, diff),
						TimeSpan = TimeHelper.TimeOfDayFromTimeSpan(layer.Start.TimeOfDay, CultureInfo.CurrentCulture) + " - " + TimeHelper.TimeOfDayFromTimeSpan(layer.End.TimeOfDay, CultureInfo.CurrentCulture)
					};
					periods.Add(scheduleLayerViewModel);
				}
				var model = new TeamScheduleAgentScheduleViewModel
				{
					Periods = periods
				};
				result.Add(model);
			}
			return result;
		}

		private static decimal calculatePosition(DateTime time , DateTime timeLineStartTime, decimal diff)
		{
			return Math.Round((time - timeLineStartTime).Ticks / diff, 4);
		}
	}
}
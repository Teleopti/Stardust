using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeAddPersonScheduleViewModel : AgentInTeamScheduleViewModel
	{
		public Guid? ShiftExchangeOfferId { get; set; }

		public ShiftTradeAddPersonScheduleViewModel() { }

		public ShiftTradeAddPersonScheduleViewModel(AgentInTeamScheduleViewModel input)
		{
			PersonId = input.PersonId;
			Name = input.Name;
			ScheduleLayers = input.ScheduleLayers;
			IsDayOff = input.IsDayOff;
			IsFullDayAbsence = input.IsFullDayAbsence;
			DayOffName = input.DayOffName;
			MinStart = input.MinStart;
			StartTimeUtc = input.StartTimeUtc;
			Total = input.Total;
			ShiftExchangeOfferId = null;
		}
	}
}
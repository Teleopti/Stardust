using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class IntradayAbsenceCategoryViewModel
	{
		public string ShortName { get; set; }
		public string Color { get; set; }
	}

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
			ContractTimeInMinute = input.ContractTimeInMinute;
		}

		public bool IsIntradayAbsence { get; set; }

		public IntradayAbsenceCategoryViewModel IntradayAbsenceCategory { get; set; }

		public string Start => this.ScheduleLayers?.ToList().FirstOrDefault()?.Start.ToString("yyyy-MM-dd HH:mm:ss");

		public string End => this.ScheduleLayers?.ToList().LastOrDefault()?.End.ToString("yyyy-MM-dd HH:mm:ss");

	}
}
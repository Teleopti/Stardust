using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeAddPersonScheduleViewModel : AgentInTeamScheduleViewModel
	{
		public Guid? ShiftExchangeOfferId { get; set; }
	}
}
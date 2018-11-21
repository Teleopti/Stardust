using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel
{
	public class AbsenceAndTextRequestViewModel : RequestViewModel
	{
		public bool IsFullDay { get; set; }
		public PersonAccountSummaryViewModel PersonAccountSummaryViewModel { get; set; }
		public IEnumerable<ShiftViewModel> Shifts { get; set; }
	}

	public class PersonAccountSummaryViewModel
	{
		public IEnumerable<PersonAccountSummaryDetailViewModel> PersonAccountSummaryDetails;
	}

	public class ShiftViewModel: AgentInTeamScheduleViewModel
	{

	}

	public class PersonAccountSummaryDetailViewModel
	{
		public DateTime StartDate { get; set; }
		public string RemainingDescription { get; set; }
		public string TrackingTypeDescription { get; set; }
		public DateTime EndDate { get; set; }
	}
}
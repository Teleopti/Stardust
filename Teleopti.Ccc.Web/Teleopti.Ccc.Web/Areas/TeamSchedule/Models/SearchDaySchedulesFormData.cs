﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class SearchDaySchedulesFormData
	{
		public Guid[] SelectedTeamIds { get; set; }
		public string Keyword { get; set; }
		public DateOnly Date { get; set; }
		public int PageSize { get; set; }
		public int CurrentPageIndex { get; set; }
		public bool IsOnlyAbsences { get; set; }
	}
}
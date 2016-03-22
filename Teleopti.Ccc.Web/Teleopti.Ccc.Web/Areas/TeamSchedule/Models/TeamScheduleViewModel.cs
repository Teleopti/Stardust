﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class PermissionsViewModel
	{
		public bool IsAddFullDayAbsenceAvailable { get; set; }
		public bool IsAddIntradayAbsenceAvailable { get; set; }
		public bool IsSwapShiftsAvailable { get; set; }
		public bool IsRemoveAbsenceAvailable { get; set; }
		public bool IsModifyScheduleAvailable { get; set; }
	}

	public class PagingGroupScheduleShiftViewModel
	{
		public IEnumerable<GroupScheduleShiftViewModel> GroupSchedule { get; set; }
		public int TotalPages { get; set; }
	}

	public class GroupScheduleViewModel
	{
		public IEnumerable<GroupScheduleShiftViewModel> Schedules { get; set; }
		public int Total { get; set; }
		public string Keyword { get; set; }
	}

	public class FullDayAbsenceForm
	{
		public IEnumerable<Guid> PersonIds { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}	
	
	public class IntradayAbsenceForm
	{
		public IEnumerable<Guid> PersonIds { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }

		public bool IsValid()
		{
			return StartTime < EndTime;
		}
	}

	public class RemovePersonAbsenceForm
	{
		public DateTime ScheduleDate { get; set; }
		public IEnumerable<Guid> PersonIds { get; set; }
		public IEnumerable<Guid> PersonAbsenceIds { get; set; }
		public bool RemoveEntireCrossDayAbsence { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}

	public class GroupScheduleInput
	{
		public IEnumerable<Guid> PersonIds { get; set; }
		public DateTime ScheduleDate { get; set; }
	}

	public class FailActionResult
	{
		public string PersonName { get; set; }
		public IList<string> Message { get; set; }
	}

	public class AgentsPerPageSettingViewModel
	{
		public int Agents { get; set; }
	}
}
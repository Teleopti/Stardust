using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		public bool HasAddingActivityPermission { get; set; }
		public bool HasRemoveActivityPermission { get; set; }
		public bool HasMoveActivityPermission { get; set; }
		public bool HasAddingPersonalActivityPermission { get; set; }
		public bool HasAddingOvertimeActivityPermission { get; set; }
		public bool HasEditShiftCategoryPermission { get; set; }
		public bool HasMoveInvalidOverlappedActivityPermission { get; set; }
		public bool HasRemoveOvertimePermission { get; set; }
		public bool HasMoveOvertimePermission { get; set; }
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

	public class GroupWeekScheduleViewModel
	{
		public List<PersonWeekScheduleViewModel> PersonWeekSchedules { get; set; } 
		public int Total { get; set; }
		public string Keyword { get; set; }
	}

	public class FullDayAbsenceForm
	{
		public IEnumerable<Guid> PersonIds { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}	
	
	public class IntradayAbsenceForm
	{
		public IEnumerable<Guid> PersonIds { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }

		public bool IsValid()
		{
			return Start < End;
		}
	}

	public class RemovePersonAbsenceForm
	{	
		public SelectedPersonAbsence[] SelectedPersonAbsences { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}

	public class SelectedPersonAbsence
	{
		public Guid PersonId { get; set; }
		public AbsenceDate[] AbsenceDates { get; set; }
	}

	public class AbsenceDate
	{
		public Guid PersonAbsenceId { get; set; }
		public DateOnly Date { get; set; }
	}

	public class GroupScheduleInput
	{
		public IEnumerable<Guid> PersonIds { get; set; }
		public DateTime ScheduleDate { get; set; }
	}

	public class ActionResult
	{
		public IList<string> ErrorMessages { get; set; }
		public IList<string> WarningMessages { get; set; }
		public Guid PersonId { get; set; }
	}

	public class AgentsPerPageSettingViewModel
	{
		public int Agents { get; set; }
	}
	public class MoveShiftForm
	{
		public Guid[] PersonIds { get; set; }
		public DateOnly Date { get; set; }
		public DateTime NewShiftStart { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}
using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


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
		public bool HasRemoveShiftPermission { get; set; }
		public bool HasAddDayOffPermission { get; set; }

		public bool HasRemoveDayOffPermission { get; set; }
		public bool HasModifyWriteProtectedSchedulePermission { get; set; }
		public bool HasExportSchedulePermission { get; set; }
	}
	
	public class GroupScheduleViewModel
	{
		public GroupScheduleViewModel() => Schedules = new List<GroupScheduleShiftViewModel>();
		public IEnumerable<GroupScheduleShiftViewModel> Schedules { get; set; }
		public int Total { get; set; }
	
	}

	public class GroupWeekScheduleViewModel
	{
		public GroupWeekScheduleViewModel() => PersonWeekSchedules = new List<PersonWeekScheduleViewModel>();
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
		public ActionResult(Guid personId)
		{
			PersonId = personId;
			ErrorMessages = new List<string>();
			WarningMessages = new List<string>();
		}
		public ActionResult() { }
		public IList<string> ErrorMessages { get; set; }
		public IList<string> WarningMessages { get; set; }
		public Guid PersonId { get; }
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

	public class AddDayOffFormData
	{
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public Guid TemplateId { get; set; }
		public Guid[] PersonIds { get; set; }
		public IDayOffTemplate Template { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}


}
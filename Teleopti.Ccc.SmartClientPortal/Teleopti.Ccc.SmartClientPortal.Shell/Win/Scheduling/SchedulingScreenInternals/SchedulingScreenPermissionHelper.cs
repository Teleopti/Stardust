using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public class SchedulingScreenPermissionHelper
	{
		public bool CheckPermission(string functionPath, ScheduleViewBase scheduleView, IEnumerable selectedEntitiesFromTreeView)
		{
			var schedulePart = scheduleView.ViewGrid[scheduleView.ViewGrid.CurrentCell.RowIndex, scheduleView.ViewGrid.CurrentCell.ColIndex].CellValue as IScheduleDay;
			if (schedulePart != null)
			{
				if (schedulePart.Person.PersonPeriodCollection.Count > 0)
				{
					IList<ITeam> teams = (from personPeriod in schedulePart.Person.PersonPeriodCollection
										  where personPeriod.Period.Contains(schedulePart.DateOnlyAsPeriod.DateOnly)
										  select personPeriod.Team).ToList();
					var permission = HasFunctionPermissionForTeams(teams, functionPath);
					return permission;
				}
			}

			return HasFunctionPermissionForTeams(selectedEntitiesFromTreeView.OfType<ITeam>(), functionPath);
		}

		public bool HasFunctionPermissionForTeams(IEnumerable<ITeam> teams, string functionPath)
		{
			var authorization = PrincipalAuthorization.Current();
			foreach (ITeam team in teams)
			{
				if (!authorization.IsPermitted(functionPath, DateOnly.Today, team))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsPermittedToViewMeeting(ScheduleViewBase scheduleView, IEnumerable selectedEntitiesFromTreeView)
		{
			const string functionPath = DefinedRaptorApplicationFunctionPaths.ModifyMeetings;
			return CheckPermission(functionPath, scheduleView, selectedEntitiesFromTreeView);
		}

		public bool IsPermittedToEditMeeting(ScheduleViewBase scheduleView, IEnumerable selectedEntitiesFromTreeView, IScenario scenario)
		{
			if (!IsPermittedToViewMeeting(scheduleView, selectedEntitiesFromTreeView)) return false;
			return !scenario.Restricted || PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);
		}

		public bool IsPermittedToWriteProtect(ScheduleViewBase scheduleView, IEnumerable selectedEntitiesFromTreeView)
		{
			const string functionPath = DefinedRaptorApplicationFunctionPaths.SetWriteProtection;
			return CheckPermission(functionPath, scheduleView, selectedEntitiesFromTreeView);
		}

		public bool IsThisPermittedApproveRequest(PersonRequestViewModel model)
		{
			return new PersonRequestAuthorization(PrincipalAuthorization.Current()).IsPermittedRequestApprove(model.RequestType);
		}

		public bool IsPermittedApproveRequest(IEnumerable<PersonRequestViewModel> models)
		{
			foreach (var model in models)
			{
				if (!IsThisPermittedApproveRequest(model)) return false;
			}
			return true;
		}

		public bool IsPermittedViewRequest()
		{
			return new PersonRequestAuthorization(PrincipalAuthorization.Current()).IsPermittedRequestView();
		}

		public bool IsViewRequestDetailsAvailable(RequestView requestView)
		{
			if (requestView.SelectedAdapters() == null || requestView.SelectedAdapters().Count != 1)
				return false;
			var selectedModel = requestView.SelectedAdapters().First();
			if (!selectedModel.IsWithinSchedulePeriod)
				return false;
			if (!IsPermittedViewRequest())
				return false;
			return true;
		}

		public bool IsPermittedToViewSchedules(IEnumerable selectedEntitiesFromTreeView)
		{
			if (HasFunctionPermissionForTeams(selectedEntitiesFromTreeView.OfType<ITeam>(), DefinedRaptorApplicationFunctionPaths.ViewSchedules)) return true;
			return false;
		}

		public void SetPermissionOnClipboardControl(ClipboardControl clipboardControl, ClipboardControl clipboardControlRestrictions)
		{
			var permissionSetter = new PermissionClipboardControl(clipboardControl);
			var permissionSetterRestriction = new PermissionClipboardRestrictionControl(clipboardControlRestrictions);
			permissionSetter.SetPermission();
			permissionSetterRestriction.SetPermission();
		}

		public void SetPermissionOnEditControl(EditControl editControl, EditControl editControlRestrictions)
		{
			var permissionSetter = new PermissionEditControl(editControl);
			var permissionSetterRestriction = new PermissionEditRestrictionControl(editControlRestrictions);
			permissionSetter.SetPermission();
			permissionSetterRestriction.SetPermission();
		}

		public void SetPermissionOnContextMenuItems(ToolStripMenuItem insertAbsence, ToolStripMenuItem insertDayOff, ToolStripMenuItem delete, ToolStripMenuItem deleteSpecial,
			ToolStripMenuItem writeProtectSchedule, ToolStripMenuItem writeProtectSchedule2, ToolStripMenuItem addStudentAvailabilityRestriction, ToolStripMenuItem addStudentAvailability,
			ToolStripMenuItem addPreferenceRestriction, ToolStripMenuItem addPreference)
		{
			var authorization = PrincipalAuthorization.Current();
			insertAbsence.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);
			insertDayOff.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			delete.Enabled = deleteSpecial.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			writeProtectSchedule.Enabled = writeProtectSchedule2.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SetWriteProtection);
			addStudentAvailabilityRestriction.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
			addStudentAvailability.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
			addPreferenceRestriction.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
			addPreference.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction);
		}

		public void SetPermissionOnMenuButtons(ToolStripButton requestView, BackStageButton options, ToolStripButton filterOvertimeAvailability, ToolStripMenuItem scheduleOvertime, ToolStripButton filterStudentAvailability)
		{
			var authorization = PrincipalAuthorization.Current();
			requestView.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RequestScheduler);
			options.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
			filterOvertimeAvailability.Visible = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities);
			scheduleOvertime.Visible = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities);
			filterStudentAvailability.Visible = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyAvailabilities);
		}

		public void SetPermissionOnScheduleControl(ToolStripEx actions, ToolStripSplitButton schedule)
		{
			actions.Enabled = true;
			schedule.Enabled = 
				PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.AutomaticScheduling);
		}

		public void CheckPastePermissions(ToolStripMenuItem paste, ToolStripMenuItem pasteSpecial)
		{
			bool permitted = PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			paste.Enabled = permitted;
			pasteSpecial.Enabled = permitted;
		}

		public void CheckModifyPermissions(ToolStripMenuItem addActivity, ToolStripMenuItem addOverTime, ToolStripMenuItem insertAbsence, ToolStripMenuItem insertDayOff )
		{
			var authorization = PrincipalAuthorization.Current();
			addActivity.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			addOverTime.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
			insertAbsence.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence);
			insertDayOff.Enabled = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment);
		}
	}
}

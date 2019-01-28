using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class PermissionEditControl
	{
		private readonly EditControl _editControl;

		public PermissionEditControl(EditControl editControl)
		{
			_editControl = editControl;
		}

		public void SetPermission()
		{
			if (_editControl != null)
			{
				var authorization = PrincipalAuthorization.Current_DONTUSE();

				_editControl.SetButtonState(EditAction.New,
											authorization.IsPermitted(
												DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment) ||
											authorization.IsPermitted(
												DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence));

				_editControl.SetSpecialItemState(EditAction.New, ClipboardItems.Shift.ToString(),
												 authorization.IsPermitted(
													 DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
				_editControl.SetSpecialItemState(EditAction.New, ClipboardItems.Absence.ToString(),
												 authorization.IsPermitted(
													 DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence));
				_editControl.SetSpecialItemState(EditAction.New, ClipboardItems.DayOff.ToString(),
												 authorization.IsPermitted(
													 DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
				_editControl.SetSpecialItemState(EditAction.New, ClipboardItems.PersonalShift.ToString(),
												 authorization.IsPermitted(
													 DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
				_editControl.SetSpecialItemState(EditAction.New, ClipboardItems.Overtime.ToString(),
												 authorization.IsPermitted(
													 DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));

				_editControl.SetButtonState(EditAction.Delete,
											authorization.IsPermitted(
												DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment) ||
											authorization.IsPermitted(
												DefinedRaptorApplicationFunctionPaths.ModifyPersonAbsence));
			}		
		}
	}
}

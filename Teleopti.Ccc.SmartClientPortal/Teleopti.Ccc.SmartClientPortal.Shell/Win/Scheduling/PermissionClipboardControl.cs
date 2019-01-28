using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class PermissionClipboardControl
	{
		private readonly ClipboardControl _clipboardControl;

		public PermissionClipboardControl(ClipboardControl clipboardControl)
		{
			_clipboardControl = clipboardControl;
		}

		public void SetPermission()
		{
			if (_clipboardControl != null)
			{
				var authorization = PrincipalAuthorization.Current_DONTUSE();
				_clipboardControl.SetButtonDropDownItemState(ClipboardAction.Cut, ClipboardItems.Shift.ToString(),
				                                             authorization.IsPermitted(
				                                             	DefinedRaptorApplicationFunctionPaths.
				                                             		ModifyPersonAssignment));
				_clipboardControl.SetButtonDropDownItemState(ClipboardAction.Cut, ClipboardItems.Absence.ToString(),
				                                             authorization.IsPermitted(
				                                             	DefinedRaptorApplicationFunctionPaths.
				                                             		ModifyPersonAbsence));
				_clipboardControl.SetButtonDropDownItemState(ClipboardAction.Cut, ClipboardItems.DayOff.ToString(),
				                                             authorization.IsPermitted(
				                                             	DefinedRaptorApplicationFunctionPaths.
																	ModifyPersonAssignment));
				_clipboardControl.SetButtonDropDownItemState(ClipboardAction.Cut,
				                                             ClipboardItems.PersonalShift.ToString(),
				                                             authorization.IsPermitted(
				                                             	DefinedRaptorApplicationFunctionPaths.
				                                             		ModifyPersonAssignment));
				_clipboardControl.SetButtonState(ClipboardAction.Cut,
				                                 authorization.IsPermitted(
				                                 	DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
				_clipboardControl.SetButtonDropDownItemState(ClipboardAction.Paste, ClipboardItems.Shift.ToString(),
				                                             authorization.IsPermitted(
				                                             	DefinedRaptorApplicationFunctionPaths.
				                                             		ModifyPersonAssignment));
				_clipboardControl.SetButtonDropDownItemState(ClipboardAction.Paste,
				                                             ClipboardItems.Absence.ToString(),
				                                             authorization.IsPermitted(
				                                             	DefinedRaptorApplicationFunctionPaths.
				                                             		ModifyPersonAbsence));
				_clipboardControl.SetButtonDropDownItemState(ClipboardAction.Paste,
				                                             ClipboardItems.DayOff.ToString(),
				                                             authorization.IsPermitted(
				                                             	DefinedRaptorApplicationFunctionPaths.
																	ModifyPersonAssignment));
				_clipboardControl.SetButtonDropDownItemState(ClipboardAction.Paste,
				                                             ClipboardItems.PersonalShift.ToString(),
				                                             authorization.IsPermitted(
				                                             	DefinedRaptorApplicationFunctionPaths.
				                                             		ModifyPersonAssignment));
				_clipboardControl.SetButtonState(ClipboardAction.Paste,
				                                 authorization.IsPermitted(
				                                 	DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment));
			}
		}
	}
}

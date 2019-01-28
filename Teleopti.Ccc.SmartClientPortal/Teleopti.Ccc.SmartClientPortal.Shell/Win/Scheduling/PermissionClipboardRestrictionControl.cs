using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public class PermissionClipboardRestrictionControl
	{
		private readonly ClipboardControl _clipboardControl;

		public PermissionClipboardRestrictionControl(ClipboardControl clipboardControl)
		{
			_clipboardControl = clipboardControl;
		}

		public void SetPermission()
		{
			if (_clipboardControl != null)
			{
				var authorization = PrincipalAuthorization.Current_DONTUSE();
				_clipboardControl.SetButtonState(ClipboardAction.Paste, authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction));
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
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
				var authorization = PrincipalAuthorization.Instance();
				_clipboardControl.SetButtonState(ClipboardAction.Paste, authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction));
			}
		}
	}
}

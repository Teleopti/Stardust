﻿using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class PermissionEditRestrictionControl
	{
		private readonly EditControl _editControl;

		public PermissionEditRestrictionControl(EditControl editControl)
		{
			_editControl = editControl;
		}

		public void SetPermission()
		{
			if (_editControl != null)
			{
				var authorization = PrincipalAuthorization.Instance();

				_editControl.SetButtonState(EditAction.New, authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction));
				_editControl.SetSpecialItemState(EditAction.New, ClipboardItems.Preference.ToString(), authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction));								
				_editControl.SetSpecialItemState(EditAction.New, ClipboardItems.StudentAvailability.ToString(),authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction));
				_editControl.SetButtonState(EditAction.Delete,authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction));
											
			}		
		}
	}
}

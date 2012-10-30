﻿using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class PasswordPage : PortalPage
	{
		[FindBy(Id = "password")]
		public TextField Password;

		[FindBy(Id = "passwordValidation")]
		public TextField PasswordValidation;

		[FindBy(Id = "oldPassword")]
		public TextField OldPassword;

		[FindBy(Id = "passwordButton")]
		public Button ConfirmButton;

		[FindBy(Id = "nonMatchingPassword")]
		public Span NonMatchingNewPassword;

		[FindBy(Id = "incorrectOldPassword")]
		public Span IncorrectPassword;

		[FindBy(Id = "updated")]
		public Label UpdatedLabel;
	}
}
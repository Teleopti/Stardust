﻿using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Authentication.Core
{
	public class ApplicationAuthentication
	{
		public ApplicationAuthenticationResult Logon(string userName, string password)
		{
			return new ApplicationAuthenticationResult
			{
				FailReason = Resources.LogOnFailedInvalidUserNameOrPassword
			};
		}
	}
}
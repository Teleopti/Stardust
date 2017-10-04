﻿namespace Teleopti.Ccc.Web.Core
{
    public interface IFormsAuthentication
    {
	    void SetAuthCookie(string userName, bool isPersistent, bool isLogonFromBrowser, string tenantName);
        void SignOut();
	    bool TryGetCurrentUser(out string userName);
    }
}
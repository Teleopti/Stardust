﻿namespace Teleopti.Ccc.Web.Core
{
    public interface IFormsAuthentication
    {
	    void SetAuthCookie(string userName);
        void SignOut();
    }
}
namespace Teleopti.Ccc.Web.Core
{
    public interface IFormsAuthentication
    {
	    void SetAuthCookie(string userName, bool isPersistent, bool isLogonFromFatClient);
        void SignOut();
	    bool TryGetCurrentUser(out string userName);
    }
}
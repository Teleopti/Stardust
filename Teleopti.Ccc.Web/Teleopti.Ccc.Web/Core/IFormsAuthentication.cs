namespace Teleopti.Ccc.Web.Core
{
    public interface IFormsAuthentication
    {
	    void SetAuthCookie(string userName, bool isPersistent);
        void SignOut();
	    bool TryGetCurrentUser(out string userName);
    }
}
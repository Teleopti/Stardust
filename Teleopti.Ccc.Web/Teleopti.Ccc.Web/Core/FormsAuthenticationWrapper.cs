using System.Web.Security;

namespace Teleopti.Ccc.Web.Core
{
	public class FormsAuthenticationWrapper : IFormsAuthentication
	{

		public void SetAuthCookie(string userName)
		{
			FormsAuthentication.SetAuthCookie(userName, false);
		}

		public void SignOut()
		{
			FormsAuthentication.SignOut();
		}
	}
}
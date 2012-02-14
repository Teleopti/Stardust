using System.Web.Security;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class FormsAuthenticationWrapper : IFormsAuthentication
	{
		public void SignOut()
		{
			FormsAuthentication.SignOut();
		}
	}
}
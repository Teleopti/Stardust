using Teleopti.Wfm.Administration.Core.Hangfire;

namespace Teleopti.Wfm.AdministrationTest
{
	public class FakeHangfireCookie : IHangfireCookie
	{
		public bool CookieIsSet;

		public void SetHangfireAdminCookie(string userName, string email)
		{
			CookieIsSet = true;
		}

		public void RemoveAdminCookie()
		{
			CookieIsSet = false;
		}
	}
}
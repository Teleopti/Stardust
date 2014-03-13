using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public interface IWindowsAccountProvider
	{
		/// <summary>
		/// Gets Windowsdomain user information  
		/// </summary>
		/// <returns>Returns null if user isn't verified by Server</returns>
		WindowsAccount RetrieveWindowsAccount();
	}
}
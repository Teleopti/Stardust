namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
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
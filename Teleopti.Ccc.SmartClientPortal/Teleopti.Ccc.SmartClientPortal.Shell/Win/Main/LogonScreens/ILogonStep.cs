namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main.LogonScreens
{
	public interface ILogonStep
	{
		void SetData();
	    void GetData();
	    void Release();
		void SetBackButtonVisible(bool visible);
	}
}

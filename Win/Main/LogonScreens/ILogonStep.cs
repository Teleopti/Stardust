namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	public interface ILogonStep
	{
		void SetData();
	    void GetData();
	    void Release();
		void SetBackButtonVisible(bool visible);
	}
}

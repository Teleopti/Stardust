namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonPresenter
	{
		void OkbuttonClicked();
		void BackButtonClicked();
		void Initialize();
		bool Start();
		LoginStep CurrentStep { get; set; }
	}
}
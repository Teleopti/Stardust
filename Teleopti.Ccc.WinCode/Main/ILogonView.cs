using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonView
	{
		LogonPresenter Presenter { get; set; }
		LoginStep CurrentStep { get; }
		void StartLogon();
		void OkButtonClicked(object data);
		void CancelButtonClicked();
		void BackButtonClicked();
		void StepForward();
		void StepBackwards();
		void Exit();
	}
}

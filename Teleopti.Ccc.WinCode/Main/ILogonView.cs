using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonView:ILicenseFeedback
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

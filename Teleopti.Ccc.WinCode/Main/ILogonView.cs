using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonView:ILicenseFeedback
	{
		ILogonPresenter Presenter { get; set; }
		bool StartLogon();
        void ShowStep(LoginStep theStep, bool showBackButton);
		void ClearForm(string labelText);
		void Exit();
	    bool InitializeAndCheckStateHolder(string skdProxyName);
	    void ShowErrorMessage(string message);
	}
}

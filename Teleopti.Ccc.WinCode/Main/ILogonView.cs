using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonView:ILicenseFeedback
	{
		ILogonPresenter Presenter { get; set; }
		bool StartLogon();
        void ShowStep(LoginStep theStep, LogonModel model, bool showBackButton);
		void ClearForm(string labelText);
		void Exit(DialogResult result);
	    bool InitializeAndCheckStateHolder(string skdProxyName);
	    void ShowErrorMessage(string message);
		void ShowWarningMessage(string message);
	}
}

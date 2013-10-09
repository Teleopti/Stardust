using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Licensing;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonView:ILicenseFeedback
	{
		ILogonPresenter Presenter { get; set; }
		bool StartLogon();
        void ShowStep(LoginStep theStep, bool showBackButton);
		void ClearForm(string labelText);
		void Exit(DialogResult result);
	    bool InitializeAndCheckStateHolder(string skdProxyName);
        void ShowErrorMessage(string message, string caption);
		void ShowWarningMessage(string message, string caption);
	    DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton);
	}
}

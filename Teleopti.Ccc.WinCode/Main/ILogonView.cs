using System;
using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonView:ILicenseFeedback
	{
		ILogonPresenter Presenter { get; set; }
		bool StartLogon(bool showDataSourceSelection);
        void ShowStep(bool showBackButton);
		void ClearForm(string labelText);
		void Exit(DialogResult result);
	    bool InitializeAndCheckStateHolder(string skdProxyName, IMessageBrokerComposite messageBroker);
        void ShowErrorMessage(string message, string caption);
		void ShowWarningMessage(string message, string caption);
	    DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton);
		void HandleKeyPress(Message msg, Keys keyData, bool b);
		void ButtonLogOnOkClick(object sender, EventArgs e);
		void ButtonLogOnCancelClick(object sender, EventArgs e);
		void BtnBackClick(object sender, EventArgs e);
		bool InitStateHolderWithoutDataSource(IMessageBrokerComposite messageBroker);
	}
}

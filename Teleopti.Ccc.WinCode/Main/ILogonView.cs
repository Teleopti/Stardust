using System;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.WinCode.Main
{
	public interface ILogonView : ILicenseFeedback
	{
		ILogonPresenter Presenter { get; set; }
		bool StartLogon(IMessageBrokerComposite messageBroker);
		void ShowStep(bool showBackButton);
		void ClearForm(string labelText);
		void Exit(DialogResult result);
		void ShowErrorMessage(string message, string caption);
		DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton);
		void HandleKeyPress(Message msg, Keys keyData, bool b);
		void ButtonLogOnOkClick(object sender, EventArgs e);
		void BtnBackClick(object sender, EventArgs e);
		void InitStateHolderWithoutDataSource(IMessageBrokerComposite messageBroker, SharedSettings settings);
		string ServerUrl { get; set; }
	}
}

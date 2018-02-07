using System;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main
{
	public interface ILogonView : ILicenseFeedback
	{
		LogonPresenter Presenter { get; set; }
		bool StartLogon();

		void ClearForm(string labelText);
		void Exit(DialogResult result);
		void ShowErrorMessage(string message, string caption);
		DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton);


		void InitStateHolderWithoutDataSource(IMessageBrokerComposite messageBroker, SharedSettings settings);
		string ServerUrl { get; set; }
	}
}

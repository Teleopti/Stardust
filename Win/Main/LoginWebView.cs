using System;
using System.Windows.Forms;
using EO.WebBrowser;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LoginWebView : MetroForm, ILogonView
	{
		private readonly LogonModel _model;

		public LoginWebView(LogonModel model)
		{
			_model = model;
			InitializeComponent();
			labelVersion.Text = string.Concat("Version ", Application.ProductVersion);
		}

		public void Warning(string warning)
		{
			throw new NotImplementedException();
		}

		public void Warning(string warning, string caption)
		{
			throw new NotImplementedException();
		}

		public void Error(string error)
		{
			throw new NotImplementedException();
		}

		public ILogonPresenter Presenter { get; set; }
		public bool StartLogon(IMessageBrokerComposite messageBroker)
		{
			webView1.RegisterJSExtensionFunction("fatClientWebLogin", WebView_JSFatClientWebLogin);
			webView1.Url = AuthenticationBridge + "/hrd";
			DialogResult result = ShowDialog();
			return result != DialogResult.Cancel;
		}

		private void WebView_JSFatClientWebLogin(object sender, JSExtInvokeArgs e)
		{
			_model.PersonId = Guid.Parse(e.Arguments[1].ToString());
			//using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			//{
			//	var businessUnit = new BusinessUnitRepository(uow).Load(Guid.Parse(e.Arguments[0].ToString()));
			//	_model.SelectedBu = businessUnit;
			//}

			Close();
			Dispose();
			Presenter.IdLogin();
		}

		public void ShowStep(bool showBackButton)
		{
			throw new NotImplementedException();
		}

		public void ClearForm(string labelText)
		{
			throw new NotImplementedException();
		}

		public void Exit(DialogResult result)
		{
			throw new NotImplementedException();
		}

		public void ShowErrorMessage(string message, string caption)
		{
			throw new NotImplementedException();
		}

		public DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton)
		{
			throw new NotImplementedException();
		}

		public void HandleKeyPress(Message msg, Keys keyData, bool b)
		{
			throw new NotImplementedException();
		}

		public void ButtonLogOnOkClick(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		public void BtnBackClick(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		public void InitStateHolderWithoutDataSource(IMessageBrokerComposite messageBroker, SharedSettings settings)
		{
			throw new NotImplementedException();
		}

		public string AuthenticationBridge { get; set; }
	}

	
}

using System;
using System.Windows.Forms;
using EO.WebBrowser;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Main;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LoginWebView : MetroForm, ILoginWebView
	{
		private readonly LogonModel _model;
		public ILogonPresenter Presenter { get; set; }

		public LoginWebView(LogonModel model)
		{
			_model = model;
			InitializeComponent();
			labelVersion.Text = string.Concat("Version ", Application.ProductVersion);
		}

		public bool StartLogon(string authenticationBridge)
		{
			webView1.RegisterJSExtensionFunction("fatClientWebLogin", WebView_JSFatClientWebLogin);
			webView1.Url = authenticationBridge + "/hrd";
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


		public void Warning(string warning)
		{
			Warning(warning, Resources.LogOn);
		}

		public void Warning(string warning, string caption)
		{
			ShowInTaskbar = true;
			MessageDialogs.ShowWarning(this, warning, caption);
			ShowInTaskbar = false;

			DialogResult = DialogResult.None;
		}

		public void Error(string error)
		{
			showApplyProductActivationKeyDialogAndExit(error);
		}

		private void showApplyProductActivationKeyDialogAndExit(string explanation)
		{
			//What should be done here

			//var applyProductActivationKey = new ApplyProductActivationKey(explanation, _model.SelectedDataSourceContainer.DataSource.Application);
			//applyProductActivationKey.ShowDialog(this);
			//Application.Exit();
		}
	}

	
}

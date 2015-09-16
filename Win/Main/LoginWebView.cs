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
		public LoginWebView()
		{
			InitializeComponent();
			labelVersion.Text = string.Concat("Version ", Application.ProductVersion);
		}

		public bool StartLogon(string authenticationBridge)
		{
			webView1.RegisterJSExtensionFunction("fatClientWebLogin", WebView_JSFatClientWebLogin);
			//webView1.Url = raptorServer + "hrd";
			webView1.Url = authenticationBridge + "/hrd";
			DialogResult result = ShowDialog();
			return result != DialogResult.Cancel;
		}

		private void WebView_JSFatClientWebLogin(object sender, JSExtInvokeArgs e)
		{
			var businessUnitId = e.Arguments[0];
			var personId = e.Arguments[1];
			Close();
			Dispose();
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

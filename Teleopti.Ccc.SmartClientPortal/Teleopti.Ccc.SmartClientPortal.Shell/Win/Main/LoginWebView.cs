using System;
using System.Configuration;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows.Forms;
using EO.Base;
using EO.WebBrowser;
using log4net;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Main;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
{
	public partial class LoginWebView : MetroForm, ILogonView
	{
		private readonly LogonModel _model;
		private readonly ILog _logger = LogManager.GetLogger(typeof(LoginWebView));
		private readonly ILog _customLogger = LogManager.GetLogger("CustomEOLogger");

		public LoginWebView(LogonModel model)
		{
			_model = model;
			InitializeComponent();
			labelVersion.Text = string.Concat("Version ", Application.ProductVersion);
			webView1.CertificateError += handlingCertificateErrors;
			EO.Base.Runtime.Exception += handlingRuntimeErrors;
			
			webView1.LoadCompleted += webView1LoadCompleted;
		}

		private void webView1LoadCompleted(object sender, LoadCompletedEventArgs e)
		{
			panel1.Visible = false;
		}

		private void logInfo(string message)
        {
            _customLogger.Info("LoginWebView: " + message);
        }

	private void handlingRuntimeErrors(object sender, ExceptionEventArgs e)
		{
			_logger.Error("Error in the EO browser", e.ErrorException);
		}

		private void handlingCertificateErrors(object sender, CertificateErrorEventArgs e)
		{
			webView1.LoadHtml($"<!doctype html><html><head></head><body>The following url is missing a certificate. <br/> {e.Url} </body></html>");
			_logger.Error("The following url is missing a certificate. " + e.Url);
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
            var applyProductActivationKey = new ApplyProductActivationKey(error, _model.SelectedDataSourceContainer.DataSource.Application);
            applyProductActivationKey.ShowDialog(this);
            Application.Exit();
		}

		public ILogonPresenter Presenter { get; set; }
		public bool StartLogon()
		{
			logInfo("EO Browser: Starting the login process by loading the URL: " + ServerUrl + "start/Url/RedirectToWebLogin");
			webView1.BeforeContextMenu += webView1_BeforeContextMenu;
			webView1.RegisterJSExtensionFunction("fatClientWebLogin", WebView_JSFatClientWebLogin);
			webView1.RegisterJSExtensionFunction("isTeleoptiProvider", WebView_JSIsTeleoptiProvider);
			logInfo("EO Browser: Loading URL to show the login web view.");
			var queryString = "";
			var queryStringFileName = @".\QueryString.txt";
			if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.ActivationUri != null)
			{
				_logger.Info($"ApplicationDeployment.CurrentDeployment.ActivationUri.Query: {ApplicationDeployment.CurrentDeployment.ActivationUri.Query}");
				queryString = ApplicationDeployment.CurrentDeployment.ActivationUri.Query.Replace("?","");
				File.WriteAllLines(queryStringFileName, new[] { queryString });
			}
			else
			{
				_logger.Info($"ApplicationDeployment.IsNetworkDeployed: {ApplicationDeployment.IsNetworkDeployed}");
				var current = Path.GetDirectoryName(Application.ExecutablePath);
				var exists = File.Exists(queryStringFileName);
				_logger.Info($"current path: {current}, check if file {queryStringFileName}: {exists}");
				if (exists)
				{
					var lines = File.ReadAllLines(queryStringFileName);
					queryString = lines.SingleOrDefault() ?? "";
					_logger.Info($"queryString read from {queryStringFileName}: {queryString}");
				}
			}

			webView1.Url = ServerUrl + "start/Url/RedirectToWebLogin?queryString=" + HttpUtility.UrlEncode(queryString);
			// some defensive coding to prevent bug 39408
			if (Visible)
				return true;
			var result = ShowDialog();
			return result != DialogResult.Cancel;
		}
		

		private void webView1_BeforeContextMenu(object sender, BeforeContextMenuEventArgs e)
		{
			e.Menu.Items.Clear();
		}

		private void WebView_JSIsTeleoptiProvider(object sender, JSExtInvokeArgs e)
		{
			logInfo("EO Browser: Called from the JS to populate the application type");
			_model.AuthenticationType = AuthenticationTypeOption.Application;
		}

		private void WebView_JSFatClientWebLogin(object sender, JSExtInvokeArgs e)
		{
			logInfo("EO Browser: Called from the JS to start the login process for fat client");
			var personId = Guid.Parse(e.Arguments[1].ToString());
			var businessUnitId = Guid.Parse(e.Arguments[0].ToString());
			_model.PersonId = personId;
			Presenter.webLogin(businessUnitId);
		}

		public void ShowStep(bool showBackButton)
		{
			throw new NotImplementedException();
		}

		public void ClearForm(string labelText)
		{
			Refresh();
		}

		public void Exit(DialogResult result)
		{
			DialogResult = result;
			Close();
			Dispose();
		}

		public void ShowErrorMessage(string message, string caption)
		{
			MessageDialogs.ShowWarning(this, message, caption);
		}

		public DialogResult ShowYesNoMessage(string text, string caption, MessageBoxDefaultButton defaultButton)
		{
			return ViewBase.ShowYesNoMessage(this, text, caption, defaultButton);
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
			LogonInitializeStateHolder.InitWithoutDataSource(messageBroker, settings);
		}

		public string ServerUrl { get; set; }

		private void loginWebViewLoad(object sender, EventArgs e)
		{
			Show();
		}
	}



	
}

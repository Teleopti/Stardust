﻿using System;
using System.Windows.Forms;
using EO.Base;
using EO.WebBrowser;
using log4net;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Main;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Main
{
	public partial class LoginWebView : MetroForm, ILogonView
	{
		private readonly LogonModel _model;
		private readonly ILog _logger = LogManager.GetLogger(typeof(LoginWebView));
		private int _restartLogonWhenEoBrowserErrorsCount; //used to control max retries of logon during EO browser errors

		public LoginWebView(LogonModel model)
		{
			_model = model;
			InitializeComponent();
			labelVersion.Text = string.Concat("Version ", Application.ProductVersion);
			webView1.CertificateError += handlingCertificateErrors;
			EO.Base.Runtime.Exception += handlingRuntimeErrors;
			_restartLogonWhenEoBrowserErrorsCount = 0;
		}

		private void handlingRuntimeErrors(object sender, ExceptionEventArgs e)
		{
			_logger.Error("Error in the EO browser", e.ErrorException);

			if (_restartLogonWhenEoBrowserErrorsCount < 5)
			{
				_restartLogonWhenEoBrowserErrorsCount++;
				StartLogon();
			}
			else
			{
				_restartLogonWhenEoBrowserErrorsCount = 0;
			}
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
			webView1.BeforeContextMenu += webView1_BeforeContextMenu;
			webView1.RegisterJSExtensionFunction("fatClientWebLogin", WebView_JSFatClientWebLogin);
			webView1.RegisterJSExtensionFunction("isTeleoptiProvider", WebView_JSIsTeleoptiProvider);
			webView1.Url = ServerUrl + "start/Url/RedirectToWebLogin";
			DialogResult result = ShowDialog();
			return result != DialogResult.Cancel;
		}
		

		private void webView1_BeforeContextMenu(object sender, BeforeContextMenuEventArgs e)
		{
			e.Menu.Items.Clear();
		}

		private void WebView_JSIsTeleoptiProvider(object sender, JSExtInvokeArgs e)
		{
			_model.AuthenticationType = AuthenticationTypeOption.Application;
		}

		private void WebView_JSFatClientWebLogin(object sender, JSExtInvokeArgs e)
		{
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
	}



	
}

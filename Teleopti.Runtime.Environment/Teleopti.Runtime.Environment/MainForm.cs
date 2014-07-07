using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;
using EO.WebBrowser;
using Teleopti.Runtime.Environment.Properties;

namespace Teleopti.Runtime.Environment
{
	public partial class MainForm : Form
	{
		private const string LoadingText = " ...";
		private const int m_AlwaysOnTopID = 0x100;
		private readonly string InitialTitle;

		public MainForm()
		{
			InitializeComponent();
			InitialTitle = Text;
		}

		public MainForm(string url)
			: this()
		{
			webView1.Url = url;
			webView1.RegisterJSExtensionFunction("envelopeNotification", WebView_JSEnvelopeNotification);
		}

		private void WebView_JSEnvelopeNotification(object sender, JSExtInvokeArgs e)
		{
			var message = e.Arguments[0] as string;
			var title = Resources.AgentScheduleMessenger;
			ShowBalloon(false, message, title);
		}

		private void webView1_NewWindow(object sender, NewWindowEventArgs e)
		{
			var form = new MainForm();
			if (e.TargetUrl.ToUpper().Contains("MYTIME/ASM"))
			{
				bool currentValue;
				if (Boolean.TryParse(ConfigurationManager.AppSettings["defaultAlwaysOnTopASM"], out currentValue))
				{
					form.TopMost = currentValue;
				}
			}
			if (e.Height.HasValue)
				form.Height = e.Height.Value;
			if (e.Width.HasValue)
				form.Width = e.Width.Value;
			form.webControl1.WebView = e.WebView;
			form.webControl1.WebView.NewWindow += form.WebView_NewWindow;
			form.webControl1.WebView.BeforeContextMenu += webView1_BeforeContextMenu;
			form.webControl1.WebView.IsLoadingChanged += webView1_IsLoadingChanged;
			e.Accepted = true;
			form.Show();
		}

		private void WebView_NewWindow(object sender, NewWindowEventArgs e)
		{
			if (e.TargetUrl.ToLower().StartsWith("http://wiki.teleopti.com"))
			{
				var sInfo = new ProcessStartInfo(e.TargetUrl);
				Process.Start(sInfo);
			}
		}

		private void webView1_BeforeContextMenu(object sender, BeforeContextMenuEventArgs e)
		{
			foreach (var item in e.Menu.Items)
			{
				if (item.CommandId == CommandIds.ViewSource)
				{
					e.Menu.Items.Remove(item);
					break;
				}
			}
		}

		private void webView1_IsLoadingChanged(object sender, EventArgs e)
		{
			if (webView1.IsLoading)
			{
				if (!Text.EndsWith(LoadingText))
					Text += LoadingText;
			}
			else
			{
				Text = InitialTitle;
			}
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			try
			{
				var systemMenu = SystemMenu.FromForm(this);
				systemMenu.InsertSeparator(0);

				systemMenu.InsertMenu(0, (TopMost ? ItemFlags.mfChecked : ItemFlags.mfUnchecked) | ItemFlags.mfString,
					m_AlwaysOnTopID, Resources.AlwaysOnTop);
			}
			catch (NoSystemMenuException)
			{
			}
		}

		protected override void WndProc(ref Message msg)
		{
			if (msg.Msg == (int)WindowMessages.wmSysCommand)
			{
				switch (msg.WParam.ToInt32())
				{
					case m_AlwaysOnTopID:
						TopMost = !TopMost;
						var systemMenu = SystemMenu.FromForm(this);
						systemMenu.ToggleMenuItem(m_AlwaysOnTopID, TopMost);
						break;
				}
			}
			base.WndProc(ref msg);
		}

		private void ShowBalloon(bool showOnlyMinimized, string message, string title)
		{
			if (showOnlyMinimized) return;
			notifyIconScheduleMessenger.BalloonTipIcon = ToolTipIcon.Info;
			notifyIconScheduleMessenger.BalloonTipText = message;
			notifyIconScheduleMessenger.BalloonTipTitle = title;
			notifyIconScheduleMessenger.ShowBalloonTip(500);
		}

		private void notifyIconScheduleMessenger_BalloonTipClicked(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Minimized;
			Show();
			WindowState = FormWindowState.Normal;
		}
	}
}

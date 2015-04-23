using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Web;
using Teleopti.Analytics.Portal.AnalyzerProxy;
using Teleopti.Analytics.Portal.PerformanceManager.Helper;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal.PerformanceManager
{
	public interface IMasterPage
	{
		string ModifyQueryString(NameValueCollection queryString);
	}

	public partial class Site1 : System.Web.UI.MasterPage, IMasterPage
	{
		protected void Page_Init(object sender, EventArgs e)
		{
			if (Context.Session == null) return;

			if (Context.Session.IsNewSession)
			{
				string timeoutUrl = string.Format(CultureInfo.InvariantCulture, "../timeout.aspx?{0}", ModifyQueryString(Request.QueryString));
				Response.Write(string.Format(CultureInfo.InvariantCulture, "<SCRIPT>top.location.href='{0}'</SCRIPT>", timeoutUrl));
				Response.End();
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			const string permissionDenyText = "Permission denied to view and create Performance Manager reports.";

			ActionBarView1.Visible = false;
			ReportTreeView1.Visible = false;

			if (AuthorizationHelper.IsAuthenticationConfigurationValid())
			{
				if (!AuthorizationHelper.DoCurrentUserHavePmPermission(AuthorizationHelper.LoggedOnUserName))
				{
					Message = permissionDenyText;
				}
				else
				{
					switch (PermissionInformation.UserPermissions)
					{
						case PermissionLevel.GeneralUser:
							ReportTreeView1.Visible = true;
							break;
						case PermissionLevel.ReportDesigner:
							ActionBarView1.Visible = true;
							ReportTreeView1.Visible = true;
							ActionBarView1.NewReportEnabled = true;
							ActionBarView1.DeleteModeEnabled = true;
							break;
						default:
							Message = permissionDenyText;
							break;
					}
				}
			}
			else
			{
				// Invalid auth configuration!!!
				Message = "The authorization configuration in the config file is invalid. Either change the authentication mode to Windows or make sure you have assigned valid values for the settings 'PM_Authentication_Mode', 'PM_Anonymous_User_Name' and 'PM_Anonymous_User_Password'.";
			}


			if (!IsPostBack)
			{
				RegisterClientWindowUnloadScript();
				ReportTreeView1.SetDeleteMode(false);
				SetUserName();
				BuildHelpLinkAction();
			}

			ActionBarView1.DeleteModeChanged += new EventHandler(ActionBarView1_DeleteModeChanged);
			ReportTreeView1.ReportDeleted += new EventHandler(ReportTreeView1_ReportDeleted);
		}

		public string CurrentReportInstanceId
		{
			get { return (string)ViewState["CurrentReportInstanceId"]; }
			set { ViewState["CurrentReportInstanceId"] = value; }
		}

		public string Message
		{
			get { return LabelMessage.Text; }
			set
			{
				LabelMessage.Visible = !string.IsNullOrEmpty(value);
				LabelMessage.Text = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public string ModifyQueryString(NameValueCollection queryString)
		{
			NameValueCollection newQs = HttpUtility.ParseQueryString(queryString.ToString());
			newQs.Remove("reportid");
			newQs.Remove("reportname");
			newQs.Remove("new");

			return newQs.ToString();
		}

		private void BuildHelpLinkAction()
		{
			string url = HelpLinkBuilder.GetPerformanceManagerHelpLink();
			ImageButtonHelp.OnClientClick = "javascript: window.open('" + url + "', 'HelpWindow', 'width=800, heigth=450, scrollbars=yes, resizable=yes') ; return false;";
		}

		private void SetUserName()
		{
			linkButtonLogOut.Text += string.Format(CultureInfo.InvariantCulture, " ({0})", AuthorizationHelper.LoggedOnUserName);
		}

		private void RegisterClientWindowUnloadScript()
		{
			var script = new StringBuilder();
			script.AppendLine("function window_onunload()");
			script.AppendLine("{");
			script.Append("var frame = document.createElement('iframe');");
			script.Append("frame.style.display = 'none';");
			script.AppendFormat("frame.src = 'CloseReport.aspx?id={0}';", CurrentReportInstanceId);
			script.Append("document.body.appendChild(frame);");
			script.AppendLine("}");

			Page.ClientScript.RegisterClientScriptBlock(GetType(), "startup_script", script.ToString(), true);
		}

		private void RegisterClientCloseWindow()
		{
			var script = new StringBuilder();
			script.Append("top.window.close();");

			Page.ClientScript.RegisterClientScriptBlock(GetType(), "close_window_script", script.ToString(), true);
		}

		void ReportTreeView1_ReportDeleted(object sender, EventArgs e)
		{
			ReportTreeView1.LoadTreeView();
		}

		void ActionBarView1_DeleteModeChanged(object sender, EventArgs e)
		{
			if (StateHolder.IsPmDeleteMode)
			{
				ReportTreeView1.SetDeleteMode(true);
				ActionBarView1.NewReportEnabled = false;
				ContentPlaceHolder3.Controls.Clear();
				return;
			}

			ReportTreeView1.SetDeleteMode(false);
			ActionBarView1.NewReportEnabled = true;
			ContentPlaceHolder3.Visible = true;
		}

		protected void linkButtonLogOut_Click(object sender, EventArgs e)
		{
			var olapInformation = new OlapInformation();

			using (var clientProxy = new ClientProxy(olapInformation.OlapServer, olapInformation.OlapDatabase))
			{
				clientProxy.LogOffUser();
			}
			Session.Abandon();
			RegisterClientCloseWindow();

			Response.Redirect(LogOutUrl());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		protected string LogOutUrl()
		{
			return string.Format("~/Logout.aspx{0}", QueryStringWithPrefix);
		}

		protected virtual string QueryStringWithPrefix
		{
			get { return string.Concat("?", Request.QueryString.ToString()); }
		}
	}
}
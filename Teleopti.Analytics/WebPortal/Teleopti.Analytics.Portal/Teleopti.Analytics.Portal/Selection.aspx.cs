using System;
using System.Data;
using System.Globalization;
using Teleopti.Analytics.Portal.Utils;
using Teleopti.Analytics.ReportTexts;

namespace Teleopti.Analytics.Portal
{
	public partial class Selection : MatrixBasePage
	{

		protected void Selector_OnInit(object sender, EventArgs e)
		{
			if (!Guid.TryParse(Request.QueryString["REPORTID"], out _reportId))
				return;
			if (!string.IsNullOrEmpty(Request.QueryString.Get("BUID")))
			{
				BusinessUnitCode = new Guid(Request.QueryString["BUID"]);
			}

			Parameter.ReportId = ReportId;
			Parameter.UserCode = UserCode;
			Parameter.BusinessUnitCode = BusinessUnitCode;
			Parameter.LanguageId = LangId;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Page.IsPostBack)
				if (Session.IsNewSession)
					Response.Redirect(string.Format(CultureInfo.InvariantCulture, "~/Timeout.aspx{0}", QueryStringWithPrefix), true);

			LoggedOnUser.Text = LoggedOnUserInformation;
			if (!Page.IsPostBack)
				checkStandardReportOrPerformanceManager();

			Parameter.ReportId = ReportId;
			Parameter.GroupPageCode = GroupPageCode;
			Parameter.UserCode = UserCode;
			Parameter.BusinessUnitCode = BusinessUnitCode;
			Parameter.LanguageId = LangId;

			SignOutButton.Text = Resources.ResSignOut;

			if (Parameter.IsReportPermissionGranted)
			{
				// User have permission on report
				DataTable tableProps = Parameter.ReportProperties;
				if (tableProps.Rows.Count > 0)
				{
					DataRow r = tableProps.Rows[0];
					string resKey = r["report_name_resource_key"].ToString();
					var caption = Resources.ResourceManager.GetString(resKey);
					if (string.IsNullOrEmpty(caption))
						caption = r["name"].ToString();
					labelRepCaption.Text = caption;
					ImageButtonHelp.ToolTip = Resources.ResHelp;

					string url = HelpLinkBuilder.GetStandardReportHelpLink((string)r["help_key"]);

					ImageButtonHelp.OnClientClick = "javascript: window.open('" + url + "', 'HelpWindow', 'width=800, heigth=450, scrollbars=yes, resizable=yes') ; return false";
					Page.Title = labelRepCaption.Text;
				}

				buttonShow.ToolTip = Resources.ResShowReport;
				CPEReports.ExpandedText = Resources.CommonCollapse;
				CPEReports.CollapsedText = Resources.CommonExpand;
			}
			else
			{
				// User do not have permission on report
				buttonShow.Visible = false;
				labelPermissionDenied.Visible = true;
				ImageButtonHelp.Visible = false;
				labelPermissionDenied.Text = Resources.ResPermissionDenied;
				labelRepCaption.Text = "";
			}
		}

		private void checkStandardReportOrPerformanceManager()
		{
			if (IsBrowseTargetPerformanceManager)
				Response.Redirect(PerformanceManagerUrl, true);

			if (ReportId != Guid.Empty)
				HiddenID.Value = ReportId.ToString();
			else
				Response.End();
		}

		protected void ButtonShowClick(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			if (Parameter.IsValid)
			{
				// Create unique key to make the two parameters session variables unique - this to avoid conflict 
				// craches when more than one report is open within the same session.
				var uniqueKey = Guid.NewGuid();
				Session.Add("PARAMETERS" + uniqueKey, Parameter.Parameters);
				Session.Add("PARAMETERTEXTS" + uniqueKey, Parameter.ParameterTexts);
				//string[] queryStringValues = { HiddenID.Value, HiddenUserID.Value, HiddenLangId.Value, HiddenCulture.Value};
				string[] queryStringValues = { HiddenID.Value, BusinessUnitCode.ToString(), HiddenUserID.Value, uniqueKey.ToString() };
				string rdlcSource =
					Page.ResolveUrl(
						String.Format(CultureInfo.CurrentCulture, "~/ReportViewer.aspx?REPORTID={0}&BUID={1}&USERID={2}&PARAMETERSKEY={3}", queryStringValues));

				//imgLoading.Visible = true;
				ViewerFrame.Attributes.Add("src", rdlcSource);
				ViewerFrame.Style.Add("display", "");
				CPEReports.Collapsed = true;
				CPEReports.ClientState = "true";
			}
		}
	}
}

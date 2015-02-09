<%@ Page Inherits="System.Web.UI.Page" Language="C#" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="Microsoft.Reporting.WebForms" %>
<%@ Import Namespace="Teleopti.Analytics.ReportTexts" %>
<%@ Import Namespace="Teleopti.Ccc.Domain.Security.Principal" %>
<%@ Import Namespace="Teleopti.Ccc.Web.Areas.Reporting.Core" %>
<%@ Register TagPrefix="Analytics" Namespace="Teleopti.Analytics.Parameters" Assembly="Teleopti.Analytics.Parameters" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<script type="text/javascript" src="../../Content/jquery/jquery-1.10.2.min.js"></script>
	<script type="text/javascript" src="Content/Scripts/persianDatepicker.min.js"></script>
	<link href="Content/Styles/persianDatepicker-default.css" rel="stylesheet" />
	<link href="Content/Styles/Styles.css" rel="stylesheet" />
	<link href="Content/Styles/calendar.css" rel="stylesheet" />
</head>

<script runat="server">
	protected Guid ReportId;
	protected Guid GroupPageCode;
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		if (!string.IsNullOrEmpty(Request.QueryString.Get("REPORTID")))
		{
			if (!Guid.TryParse(Request.QueryString["REPORTID"], out ReportId))
				return;
			ParameterSelector.ReportId = ReportId;

			if (ParameterSelector.IsReportPermissionGranted)
			{
				var commonReports = new CommonReports(ParameterSelector.ConnectionString, ParameterSelector.ReportId);
				Guid groupPageComboBoxControlCollectionId = commonReports.GetGroupPageComboBoxControlCollectionId();
				string groupPageComboBoxControlCollectionIdName = string.Format("ParameterSelector$Drop{0}", groupPageComboBoxControlCollectionId);

				GroupPageCode = string.IsNullOrEmpty(Request.Form.Get(groupPageComboBoxControlCollectionIdName))
											? Selector.BusinessHierarchyCode
											: new Guid(Request.Form.Get(groupPageComboBoxControlCollectionIdName));
				ParameterSelector.GroupPageCode = GroupPageCode;
				commonReports.LoadReportInfo();
				Page.Header.Title = commonReports.Name;
				//labelRepCaption.Text = commonReports.Name;
			}
			else
			{
				// User do not have permission on report
				buttonShowExcel.Visible = false;
				buttonShowPdf.Visible = false;
				labelPermissionDenied.Visible = true;
				ParameterSelector.Visible = false;
				labelPermissionDenied.Text = Resources.ResPermissionDenied;
				Page.Header.Title = Resources.ResPermissionDenied;
			}
		}
	}

	protected void Selector_OnInit(object sender, EventArgs e)
	{
		var princip = Thread.CurrentPrincipal;
		var id = ((TeleoptiPrincipalCacheable)princip).Person.Id;
		var dataSource = ((TeleoptiIdentity)princip.Identity).DataSource;
		var bu = ((TeleoptiIdentity)princip.Identity).BusinessUnit.Id;

		ParameterSelector.ConnectionString = dataSource.Statistic.ConnectionString;
		ParameterSelector.UserCode = id.GetValueOrDefault();
		ParameterSelector.BusinessUnitCode = bu.GetValueOrDefault();
		ParameterSelector.LanguageId = ((TeleoptiPrincipalCacheable) princip).Person.PermissionInformation.UICulture().LCID;

	}

	private void buttonShowClickPdf(object sender, ImageClickEventArgs e)
	{
		createReport("PDF");
	}

	private void buttonShowClickExcel(object sender, ImageClickEventArgs e)
	{
		createReport("Excel");
	}

	private void buttonShowClickWord(object sender, ImageClickEventArgs e)
	{
		createReport("Word");
	}
	private void createReport(string format)
	{
		//aspnetForm.Target = "_blank";
		var commonReports = new CommonReports(ParameterSelector.ConnectionString, ParameterSelector.ReportId);

		var sqlParams = ParameterSelector.Parameters;
		var texts = ParameterSelector.ParameterTexts;
		commonReports.LoadReportInfo();
		DataSet dataset = commonReports.GetReportData(ParameterSelector.UserCode, ParameterSelector.BusinessUnitCode, sqlParams);
		
		string reportName = commonReports.ReportFileName;
		string reportPath = Server.MapPath(reportName);
		IList<ReportParameter> @params = new List<ReportParameter>();
		var viewer = new ReportViewer {ProcessingMode = ProcessingMode.Local};
		viewer.LocalReport.ReportPath = reportPath;
		ReportParameterInfoCollection repInfos = viewer.LocalReport.GetParameters();

		foreach (ReportParameterInfo repInfo in repInfos)
		{
			int i = 0;
			var added = false;
			foreach (SqlParameter param in sqlParams)
			{
				if (repInfo.Name.StartsWith("Res", StringComparison.CurrentCultureIgnoreCase))
				{
					@params.Add(new ReportParameter(repInfo.Name, Resources.ResourceManager.GetString(repInfo.Name), false));
					added = true;
				}
				if (param.ParameterName.ToLower(CultureInfo.CurrentCulture) == "@" + repInfo.Name.ToLower(CultureInfo.CurrentCulture))
				{
					@params.Add(new ReportParameter(repInfo.Name, texts[i], false));
					added = true;
				}
				if (!added && repInfo.Name == "culture")
					@params.Add(new ReportParameter("culture", Thread.CurrentThread.CurrentCulture.IetfLanguageTag, false));
				i += 1;
			}
		}

		viewer.LocalReport.SetParameters(@params);

		//The first in the report has to have the name DataSet1
		dataset.Tables[0].TableName = "DataSet1";
		foreach (DataTable t in dataset.Tables)
		{
			viewer.LocalReport.DataSources.Add(new ReportDataSource(t.TableName, t));
		}
		// Variables
		Warning[] warnings;
		string[] streamIds;
		string mimeType;
		string encoding;
		string extension;

		// Setup the report viewer object and get the array of bytes
		byte[] bytes = viewer.LocalReport.Render(format, null, out mimeType, out encoding, out extension, out streamIds, out warnings);

		// Now that you have all the bytes representing the PDF report, buffer it and send it to the client.
		Response.Buffer = true;
		Response.Clear();
		Response.ContentType = mimeType;
		Response.AddHeader("content-disposition", "inline; filename=" + reportName + "." + extension);
		Response.BinaryWrite(bytes); // create the file
		Response.Flush(); // send it to the client to download
	}
</script>

<body >
	<%--<div class="Caption">
		<div class="ReportName" style="padding-top: 2px">
			<asp:Label ID="labelRepCaption" runat="server" Text=""></asp:Label>
		</div>
	</div>--%>
	<form id="aspnetForm" style="margin-top: 50px" runat="server">
		<asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server" EnableScriptGlobalization="true" EnableScriptLocalization="true" />
		<div class="Panel">
			<div class="DetailsView" style="height: 80%; overflow: auto">
				<Analytics:Selector LabelWidth="30%" List1Width="75%" ID="ParameterSelector" name="ParameterSelector" runat="server" OnInit="Selector_OnInit"></Analytics:Selector>
				<div style="float: left; width: 29%">
					<asp:ValidationSummary ID="ValidationSummary1" runat="server" ForeColor="Red" />
					<asp:Label ID="labelPermissionDenied" runat="server" ForeColor="Red" Font-Size="Large" Visible="false"></asp:Label>
				</div>
				<div style="float: right; width: 69%">
					<div style="float: left; width: 33%;">
						<asp:ImageButton Style="float: right;margin-right: 25px" formtarget="_blank" OnClick="buttonShowClickPdf" ID="buttonShowPdf" Width="48" Height="48" ImageUrl="images/filetype_pdf.png" ToolTip='Show PDF report' runat="server" />
					</div>
					<div style="float: right; width: 33%">
						<asp:ImageButton Style="float: left" formtarget="_blank" OnClick="buttonShowClickExcel" ID="buttonShowExcel" Width="48" Height="48" ImageUrl="images/excel.png" ToolTip='Show Excel report' runat="server" />
					</div>
					<div style="float: right; width: 20%">
						<asp:ImageButton Style="float: left" formtarget="_blank" OnClick="buttonShowClickWord" ID="buttonShowWord" Width="48" Height="48" ImageUrl="images/icon.doc.png" ToolTip='Show Word report' runat="server" />
					</div>
				</div>
			</div>
	</div>
	</form>
</body>
</html>

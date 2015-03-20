using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using System.Web.UI;
using Microsoft.Reporting.WebForms;
using Teleopti.Analytics.Parameters;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

namespace Teleopti.Ccc.Web.Areas.Reporting
{
	public partial class Index :Page
	{
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
					buttonShowExcel.ToolTip = UserTexts.Resources.ShowExcelReport;
					buttonShowWord.ToolTip = UserTexts.Resources.ShowWordReport;
					buttonShowPdf.ToolTip = UserTexts.Resources.ShowPDFReport;
				}
				else
				{
					// User do not have permission on report
					buttonShowExcel.Visible = false;
					buttonShowPdf.Visible = false;
					buttonShowWord.Visible = false;
					ParameterSelector.Visible = false;
					if (ReportId.Equals(Guid.Empty)) return;
					labelPermissionDenied.Visible = true;
					labelPermissionDenied.Text = Resources.ResPermissionDenied;
					Page.Header.Title = Resources.ResPermissionDenied;
				}
				
			}
		}


		protected void Selector_OnInit(object sender, EventArgs e)
		{
			var princip = Thread.CurrentPrincipal;
			var person = ((TeleoptiPrincipalCacheable)princip).Person;
			var id = person.Id;
			var dataSource = ((TeleoptiIdentity)princip.Identity).DataSource;
			var bu = ((TeleoptiIdentity)princip.Identity).BusinessUnit.Id;

			Thread.CurrentThread.CurrentUICulture = person.PermissionInformation.UICulture().FixPersianCulture();
			Thread.CurrentThread.CurrentCulture = person.PermissionInformation.Culture().FixPersianCulture();

			ParameterSelector.ConnectionString = dataSource.Statistic.ConnectionString;
			ParameterSelector.UserCode = id.GetValueOrDefault();
			ParameterSelector.BusinessUnitCode = bu.GetValueOrDefault();
			ParameterSelector.LanguageId = ((TeleoptiPrincipalCacheable)princip).Person.PermissionInformation.UICulture().LCID;

		}

		protected override void OnLoadComplete(EventArgs e)
		{
			base.OnLoadComplete(e);
			buttonShowExcel.Enabled = ParameterSelector.IsValid;
			buttonShowPdf.Enabled = ParameterSelector.IsValid;
			buttonShowWord.Enabled = ParameterSelector.IsValid;
		}

		protected void ButtonShowClickPdf(object sender, ImageClickEventArgs e)
		{
			createReport("PDF");
		}

		protected void ButtonShowClickExcel(object sender, ImageClickEventArgs e)
		{
			createReport("Excel");
		}

		protected void ButtonShowClickWord(object sender, ImageClickEventArgs e)
		{
			createReport("Word");
		}
		protected void ButtonShowClickImage(object sender, ImageClickEventArgs e)
		{
			createReport("Image");
		}
		
		private void createReport(string format)
		{
			if (!ParameterSelector.IsValid) return;
			//aspnetForm.Target = "_blank";
			var commonReports = new CommonReports(ParameterSelector.ConnectionString, ParameterSelector.ReportId);

			var sqlParams = ParameterSelector.Parameters;
			var texts = ParameterSelector.ParameterTexts;
			commonReports.LoadReportInfo();
			DataSet dataset = commonReports.GetReportData(ParameterSelector.UserCode, ParameterSelector.BusinessUnitCode, sqlParams);

			string reportName = commonReports.ReportFileName.Replace("~/","");
			string reportPath = Server.MapPath(reportName);
			IList<ReportParameter> @params = new List<ReportParameter>();
			var viewer = new ReportViewer { ProcessingMode = ProcessingMode.Local };
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
			var inlineOrAtt = "inline;";
			if(format == "Word")
				inlineOrAtt = "attachment;";
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
			Response.AddHeader("content-disposition", inlineOrAtt + " filename=" + reportName + "." + extension);
			Response.BinaryWrite(bytes); // create the file
			Response.Flush(); // send it to the client to download
		}

		
	}
}

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
		protected bool UseOpenXml = false;
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			if (!string.IsNullOrEmpty(Request.QueryString.Get("REPORTID")))
			{
				if (!Guid.TryParse(Request.QueryString["REPORTID"], out ReportId))
					return;
				ParameterSelector.ReportId = ReportId;

				bool.TryParse(Request.QueryString.Get("UseOpenXml"), out UseOpenXml);

				using (var commonReports = new CommonReports(ParameterSelector.ConnectionString, ReportId))
				{
					Guid groupPageComboBoxControlCollectionId = commonReports.GetGroupPageComboBoxControlCollectionId();
					string groupPageComboBoxControlCollectionIdName = string.Format("ParameterSelector$Drop{0}",
						groupPageComboBoxControlCollectionId);

					GroupPageCode = string.IsNullOrEmpty(Request.Form.Get(groupPageComboBoxControlCollectionIdName))
						? Selector.BusinessHierarchyCode
						: new Guid(Request.Form.Get(groupPageComboBoxControlCollectionIdName));
					ParameterSelector.GroupPageCode = GroupPageCode;
					commonReports.LoadReportInfo();
					Page.Header.Title = commonReports.Name;
				}
				if (ReportId.Equals(Guid.Empty))
				{
					buttonShowExcel.Visible = false;
					buttonShowImage.Visible = false;
					buttonShowPdf.Visible = false;
					buttonShowWord.Visible = false;
				}
				buttonShowExcel.ToolTip = UserTexts.Resources.ShowExcelReport;
				buttonShowWord.ToolTip = UserTexts.Resources.ShowWordReport;
				buttonShowPdf.ToolTip = UserTexts.Resources.ShowPDFReport;
				
			}
		}

		protected void Selector_OnInit(object sender, EventArgs e)
		{
			if (!Request.IsAuthenticated)
			{
				if (!Guid.TryParse(Request.QueryString["REPORTID"], out ReportId))
					return;
				Response.Redirect(string.Format("~/Reporting/Report/{0}#{1}", ReportId, ReportId));
			}
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
			using (var commonReports = new CommonReports(ParameterSelector.ConnectionString, ReportId))
			{
				ParameterSelector.DbTimeout = commonReports.DbTimeout;
			}
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
			doIt("PDF");
		}

		protected void ButtonShowClickExcel(object sender, ImageClickEventArgs e)
		{
			doIt(excelFormat());
      }

		protected void ButtonShowClickWord(object sender, ImageClickEventArgs e)
		{
			doIt(wordFormat());
		}

		protected void ButtonShowClickImage(object sender, ImageClickEventArgs e)
		{
			doIt("Image");
		}

		private void doIt(string format)
		{
			try
			{
				createReport(format);
			}
			catch (SqlException exception)
			{
				aspnetForm.Visible = false;
				//timeout?
				if (exception.Number == -2)
				{
					labelError.Text = UserTexts.Resources.ReportTimeoutMessage;
					return;
				}
				labelError.Text = exception.Message;
			}
		}

		private void createReport(string format)
		{
			if (!ParameterSelector.IsValid) return;
			//aspnetForm.Target = "_blank";
			using (var commonReports = new CommonReports(ParameterSelector.ConnectionString, ReportId))
			{
				var sqlParams = ParameterSelector.Parameters;
				var texts = ParameterSelector.ParameterTexts;
				commonReports.LoadReportInfo();
				DataSet dataset = commonReports.GetReportData(ParameterSelector.UserCode, ParameterSelector.BusinessUnitCode,
					sqlParams);

				string reportName = commonReports.ReportFileName.Replace("~/", "");
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
						if (param.ParameterName.ToLower(CultureInfo.CurrentCulture) ==
						    "@" + repInfo.Name.ToLower(CultureInfo.CurrentCulture))
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
				if (format.Equals(wordFormat()) || format.Equals(excelFormat()))
					inlineOrAtt = "attachment;";
				// Variables
				Warning[] warnings;
				string[] streamIds;
				string mimeType;
				string encoding;
				string extension;

				// Setup the report viewer object and get the array of bytes
				byte[] bytes = viewer.LocalReport.Render(format, "<DeviceInfo><OutputFormat>PNG</OutputFormat></DeviceInfo>", out mimeType, out encoding, out extension, out streamIds,
					out warnings);

				Response.Clear();
				Response.ContentType = mimeType;
				// commonReports + Guid.NewGuid() = to get uniquie name to be able to open more than one with different selections
				Response.AddHeader("content-disposition",
					inlineOrAtt + " filename=" + commonReports.Name + Guid.NewGuid() + "." + extension);
				Response.BinaryWrite(bytes); // create the file
				if(Response.IsClientConnected)
				{
					Response.Flush(); // Sends all currently buffered output to the client.
					Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
					Context.ApplicationInstance.CompleteRequest(); // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event.
				}
			}
		}
		
		private string wordFormat()
		{
			return UseOpenXml ? "WORDOPENXML" : "WORD";
		}

		private string excelFormat()
		{
			return UseOpenXml ? "EXCELOPENXML" : "EXCEL";
		}
	}
}

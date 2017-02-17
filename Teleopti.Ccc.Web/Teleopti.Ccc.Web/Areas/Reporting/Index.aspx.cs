﻿using System;
using System.Data.SqlClient;
using System.Threading;
using System.Web;
using System.Web.UI;
using Teleopti.Analytics.Parameters;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

namespace Teleopti.Ccc.Web.Areas.Reporting
{
	public partial class Index : Page
	{
		protected Guid ReportId;
		protected Guid GroupPageCode;
		protected bool UseOpenXml;
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

			ParameterSelector.ConnectionString = dataSource.Analytics.ConnectionString;
			ParameterSelector.UserCode = id.GetValueOrDefault();
			ParameterSelector.BusinessUnitCode = bu.GetValueOrDefault();
			ParameterSelector.LanguageId = ((TeleoptiPrincipalCacheable)princip).Person.PermissionInformation.UICulture().LCID;
			ParameterSelector.UserTimeZone = person.PermissionInformation.DefaultTimeZone();
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
			doIt(ReportGenerator.ReportFormat.Pdf);
		}

		protected void ButtonShowClickExcel(object sender, ImageClickEventArgs e)
		{
			doIt(UseOpenXml ? ReportGenerator.ReportFormat.ExcelOpenXml : ReportGenerator.ReportFormat.Excel);
		}

		protected void ButtonShowClickWord(object sender, ImageClickEventArgs e)
		{
			doIt(UseOpenXml ? ReportGenerator.ReportFormat.WordOpenXml : ReportGenerator.ReportFormat.Word);
		}

		protected void ButtonShowClickImage(object sender, ImageClickEventArgs e)
		{
			doIt(ReportGenerator.ReportFormat.Image);
		}

		private void doIt(ReportGenerator.ReportFormat format)
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

		private void createReport(ReportGenerator.ReportFormat format)
		{
			if (!ParameterSelector.IsValid) return;

			var reportGenerator = new ReportGenerator(new ServerPathProvider());
			
			var generatedReport = reportGenerator.GenerateReport(ReportId, ParameterSelector.ConnectionString,
				ParameterSelector.Parameters, ParameterSelector.ParameterTexts, ParameterSelector.UserCode,
				ParameterSelector.BusinessUnitCode, format, ParameterSelector.UserTimeZone);

			Response.Clear();
			Response.ContentType = generatedReport.MimeType;

			var uniqueFileName = generatedReport.ReportName + Guid.NewGuid();
			var inlineOrAtt = getInlineOrAttachment(format);

			Response.AddHeader("content-disposition",
				inlineOrAtt + " filename=" + uniqueFileName + "." + generatedReport.Extension);
			Response.AppendCookie(new HttpCookie("fileIsDownloadedToken", "itsDownloaded") { HttpOnly = false, Shareable = true});
			Response.BinaryWrite(generatedReport.Bytes); // create the file
			if (Response.IsClientConnected)
			{
				Response.Flush(); // Sends all currently buffered output to the client.
				Response.SuppressContent = true;  // Gets or sets a value indicating whether to send HTTP content to the client.
				Context.ApplicationInstance.CompleteRequest(); // Causes ASP.NET to bypass all events and filtering in the HTTP pipeline chain of execution and directly execute the EndRequest event.
			}
		}

		private static string getInlineOrAttachment(ReportGenerator.ReportFormat format)
		{
			if (format == ReportGenerator.ReportFormat.Excel || format == ReportGenerator.ReportFormat.ExcelOpenXml ||
				format == ReportGenerator.ReportFormat.Word || format == ReportGenerator.ReportFormat.WordOpenXml)
			{
				return "attachment;";
			}
			return "inline;";
		}
	}
}

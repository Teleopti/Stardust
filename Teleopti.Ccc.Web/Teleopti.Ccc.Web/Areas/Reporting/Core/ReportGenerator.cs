﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Microsoft.Reporting.WebForms;
using Teleopti.Analytics.ReportTexts;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class ReportGenerator : IReportGenerator
	{
		private readonly IPathProvider _pathProvider;
		public ReportGenerator(IPathProvider pathProvider)
		{
			_pathProvider = pathProvider;
		}

		public GeneratedReport GenerateReport(Guid reportId, string connectionString, IList<SqlParameter> parameters, IList<string> paramtersText, Guid userCode, Guid businessUnitCode, ReportFormat format)
		{
			using (var commonReports = new CommonReports(connectionString, reportId))
			{
				commonReports.LoadReportInfo();
				var dataset = commonReports.GetReportData(userCode, businessUnitCode, parameters);

				var reportName = commonReports.ReportFileName.Replace("~/", "");
				var reportPath = _pathProvider.MapPath(reportName);
				IList<ReportParameter> @params = new List<ReportParameter>();
				var viewer = new ReportViewer { ProcessingMode = ProcessingMode.Local };
				viewer.LocalReport.ReportPath = reportPath;
				var repInfos = viewer.LocalReport.GetParameters();

				foreach (var repInfo in repInfos)
				{
					var i = 0;
					var added = false;
					foreach (var param in parameters)
					{
						if (repInfo.Name.StartsWith("Res", StringComparison.CurrentCultureIgnoreCase))
						{
							@params.Add(new ReportParameter(repInfo.Name, Resources.ResourceManager.GetString(repInfo.Name), false));
							added = true;
						}
						if (param.ParameterName.ToLower(CultureInfo.CurrentCulture) == "@" + repInfo.Name.ToLower(CultureInfo.CurrentCulture))
						{
							@params.Add(new ReportParameter(repInfo.Name, paramtersText[i], false));
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
				byte[] bytes = viewer.LocalReport.Render(formatToString(format), "<DeviceInfo><OutputFormat>PNG</OutputFormat></DeviceInfo>", out mimeType, out encoding, out extension, out streamIds,
					out warnings);

				return new GeneratedReport
				{
					ReportName = commonReports.Name,
					MimeType = mimeType,
					Bytes = bytes,
					Extension = extension
				};
			}
		}

		private static string formatToString(ReportFormat format)
		{
			switch (format)
			{
				case ReportFormat.Pdf:
					return "PDF";
				case ReportFormat.Image:
					return "Image";
				case ReportFormat.Word:
					return "WORD";
				case ReportFormat.WordOpenXml:
					return "WORDOPENXML";
				case ReportFormat.Excel:
					return "EXCEL";
				case ReportFormat.ExcelOpenXml:
					return "EXCELOPENXML";
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		public enum ReportFormat
		{
			Pdf,
			Image,
			Word,
			WordOpenXml,
			Excel,
			ExcelOpenXml,
		}
	}
}
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using log4net;

namespace Teleopti.Ccc.Payroll
{
	public class TeleoptiActivitiesExport : IPayrollExportProcessorWithFeedback
	{
		private static readonly PayrollFormatDto Format =
			new PayrollFormatDto(new Guid("{0e531434-a463-4ab6-8bf1-4696ddc9b296}"), "Teleopti Activities Export");

		private static readonly ILog Logger = LogManager.GetLogger(typeof (TeleoptiActivitiesExport));
		private const int batchSize = 50;

		public IPayrollExportFeedback PayrollExportFeedback { get; set; }

		public PayrollFormatDto PayrollFormat
		{
			get { return Format; }
		}

		public IXPathNavigable ProcessPayrollData(ITeleoptiSchedulingService schedulingService,
		                                          ITeleoptiOrganizationService organizationService,
		                                          PayrollExportDto payrollExport)
		{
			using (var payrollDt = new DataTable("PayrollExport"))
			{
				addColumnsToTable(payrollDt);

				var startDateOnlyDto = payrollExport.DatePeriod.StartDate;
				var endDateOnlyDto = payrollExport.DatePeriod.EndDate;
				PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture,
				                                         "Running Teleopti Activities Export for dates {0} to {1}",
				                                         startDateOnlyDto.DateTime, endDateOnlyDto.DateTime));

				var count = payrollExport.PersonCollection.Count;
				PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture,
				                                         "Running Export for {0} persons in batches of {1}", count, batchSize));

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				var step = getIncreasePercentagePerBatch(count);
				var progress = 10;

				for (var i = 0; i < count; i += batchSize)
				{
					var currentAgents = payrollExport.PersonCollection.Skip(i).Take(batchSize).ToArray();
					var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(payrollExport.TimeZoneId);

					PayrollExportFeedback.ReportProgress(progress, "Loading data...");
					var activityExportDtos = schedulingService.GetTeleoptiActivitiesExportData(currentAgents, startDateOnlyDto,
					                                                                           endDateOnlyDto, timeZoneInfo.Id);

					foreach (var dto in activityExportDtos)
					{
						payrollDt.LoadDataRow(
							new object[]
								{
									dto.EmploymentNumber,
									dto.FirstName,
									dto.LastName,
									dto.BusinessUnitName,
									dto.SiteName,
									dto.TeamName,
									dto.ContractName,
									dto.PartTimePercentageName,
									dto.PartTimePercentageNumber,
									dto.Date,
									dto.StartDate,
									dto.EndDate,
									dto.PayrollCode,
									dto.ContractTime,
									dto.WorkTime,
									dto.PaidTime,
									dto.Time
								}, true);
					}
					progress += step;
				}

				var builder = new StringBuilder();
				using (var stringWriter = new StringWriter(builder, CultureInfo.InvariantCulture))
				{
					payrollDt.WriteXml(stringWriter, XmlWriteMode.IgnoreSchema, false);
					stringWriter.Flush();
				}

				var document = new XmlDocument();
				document.LoadXml(builder.ToString());

				Logger.Debug("Appending format to export");
				var result = FormatAppender.AppendFormat(document, "TeleoptiActivitiesExportFormat.xml");
				Logger.Debug("Done appending format to export");
				Logger.Debug("Done with payroll export");

				stopwatch.Stop();
				PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture, "The payroll export took {0} to complete.",
				                                         stopwatch.Elapsed));

				var fileName = @"C:\activityExport.xml";
				document.Save(fileName);

				return result;
			}
		}

		private static void addColumnsToTable(DataTable payrollDt)
		{
			payrollDt.Locale = CultureInfo.InvariantCulture;
			payrollDt.Columns.Add("Person", typeof (string));
			payrollDt.Columns.Add("FirstName", typeof (string));
			payrollDt.Columns.Add("LastName", typeof (string));
			payrollDt.Columns.Add("BusinessUnitName", typeof (string));
			payrollDt.Columns.Add("SiteName", typeof (string));
			payrollDt.Columns.Add("TeamName", typeof (string));
			payrollDt.Columns.Add("ContractName", typeof (string));
			payrollDt.Columns.Add("PartTimePercentageName", typeof (string));
			payrollDt.Columns.Add("PartTimePercentageNumber", typeof (double));
			payrollDt.Columns.Add("Date", typeof (DateTime));
			payrollDt.Columns.Add("Start", typeof (DateTime));
			payrollDt.Columns.Add("End", typeof (DateTime));
			payrollDt.Columns.Add("PayrollCode", typeof (string));
			payrollDt.Columns.Add("ContractTime", typeof (TimeSpan));
			payrollDt.Columns.Add("WorkTime", typeof (TimeSpan));
			payrollDt.Columns.Add("PaidTime", typeof (TimeSpan));
			payrollDt.Columns.Add("Time", typeof (TimeSpan));
		}

		private static int getIncreasePercentagePerBatch(int count)
		{
			var batchCount = Math.Max(count/batchSize, 1);
			return 80/batchCount;
		}

	}
}
using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using log4net;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Payroll
{
	public class TeleoptiTimeExport : IPayrollExportProcessorWithFeedback
	{
		private static readonly PayrollFormatDto Format =
			new PayrollFormatDto(new Guid("{5A888BEC-5954-466d-B245-639BFEDA1BB5}"), "Teleopti Time Export");

		private static readonly ILog Logger = LogManager.GetLogger(typeof (TeleoptiTimeExport));
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
				                                         "Running Teleopti Time Export for dates {0} to {1}",
				                                         startDateOnlyDto.DateTime, endDateOnlyDto.DateTime));

				var count = payrollExport.PersonCollection.Count;
				PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture,
				                                         "Running Export for {0} persons in batches of {1}", count,
				                                         batchSize));

				var stopwatch = new Stopwatch();
				stopwatch.Start();

				int step = getIncreasePercentagePerBatch(count);
				var progress = 10;

				for (int i = 0; i < count; i = i + batchSize)
				{
					PayrollExportFeedback.ReportProgress(progress, "Loading data...");

					var currentAgents = payrollExport.PersonCollection.Skip(i).Take(batchSize).ToArray();
					var personTimeZone = TimeZoneInfo.FindSystemTimeZoneById(payrollExport.TimeZoneId);

					var payrollTimeExportDataList =
						schedulingService.GetTeleoptiTimeExportData(currentAgents,

						                                            startDateOnlyDto,
						                                            endDateOnlyDto,
						                                            personTimeZone.Id);


					foreach (var payrollTimeExportData in payrollTimeExportDataList)
					{
						payrollDt.LoadDataRow(
							new object[]
								{
									payrollTimeExportData.EmploymentNumber,
									payrollTimeExportData.FirstName,
									payrollTimeExportData.LastName,
									payrollTimeExportData.BusinessUnitName,
									payrollTimeExportData.SiteName,
									payrollTimeExportData.TeamName,
									payrollTimeExportData.ContractName,
									payrollTimeExportData.PartTimePercentageName,
									payrollTimeExportData.Date,
									payrollTimeExportData.StartDate,
									payrollTimeExportData.EndDate,
									payrollTimeExportData.ShiftCategoryName,
									payrollTimeExportData.ContractTime,
									payrollTimeExportData.WorkTime,
									payrollTimeExportData.PaidTime,
									payrollTimeExportData.AbsencePayrollCode,
									payrollTimeExportData.DayOffPayrollCode
								},
							true);
					}

					progress += step;
				}

				Logger.Debug("Done with processing data");
				var builder = new StringBuilder();

				using (
					var stringWriter = new StringWriter(builder, CultureInfo.InvariantCulture)
					)
				{
					Logger.Debug("Creating XML structure from data");
					payrollDt.WriteXml(stringWriter, XmlWriteMode.IgnoreSchema, false);
					stringWriter.Flush();
					Logger.Debug("Done with creating XML structure from data");
				}

				var document = new XmlDocument();
				document.LoadXml(builder.ToString());
				Logger.Debug("Appending format to export");
				var result = FormatAppender.AppendFormat(document, "TeleoptiTimeExportFormat.xml");
				Logger.Debug("Done with appending format to export");
				Logger.Debug("Done with payroll export");

				stopwatch.Stop();
				PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture, "The payroll export took {0} to complete.",
				                                         stopwatch.Elapsed));

				var fileName = @"C:\timeExport.xml";
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
			payrollDt.Columns.Add("Date", typeof (DateTime));
			payrollDt.Columns.Add("StartDate", typeof (DateTime));
			payrollDt.Columns.Add("EndDate", typeof (DateTime));
			payrollDt.Columns.Add("ShiftCategoryName", typeof (string));
			payrollDt.Columns.Add("Time", typeof (TimeSpan));
			payrollDt.Columns.Add("WorkTime", typeof (TimeSpan));
			payrollDt.Columns.Add("PaidTime", typeof (TimeSpan));
			payrollDt.Columns.Add("AbsencePayrollCode", typeof (string));
			payrollDt.Columns.Add("DayOffPayrollCode", typeof (string));


		}

		private static int getIncreasePercentagePerBatch(int count)
		{
			var batchCount = Math.Max(count/batchSize, 1);
			return 80/batchCount;
		}
	}
}
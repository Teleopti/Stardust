using System;
using System.Collections.Generic;
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
    public class TeleoptiDetailedExport : IPayrollExportProcessorWithFeedback
    {
        private static readonly PayrollFormatDto Format = new PayrollFormatDto(new Guid("{605B87C4-B98A-4FE1-9EA2-9B7308CAA947}"), "Teleopti Detailed Export");
        private readonly static ILog Logger = LogManager.GetLogger(typeof(TeleoptiDetailedExport));
        private DataTable _payrollDt;
        private DateOnlyDto _startDateOnlyDto;
        private DateOnlyDto _endDateOnlyDto;
        private const int batchSize = 50;

        public PayrollFormatDto PayrollFormat
        {
            get { return Format; }
        }

	    public IXPathNavigable ProcessPayrollData(ITeleoptiSchedulingService schedulingService,
	                                              ITeleoptiOrganizationService organizationService,
	                                              PayrollExportDto payrollExport)
        {
            using(_payrollDt = new DataTable("PayrollExport"))
            {
                addColumnsToTable();

                Logger.Debug("Preparing cache for absences and persons");
                Logger.Debug("Done with preparing cache for absences and persons");

                _startDateOnlyDto = payrollExport.DatePeriod.StartDate;
                _endDateOnlyDto = payrollExport.DatePeriod.EndDate;

                PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture,
                                                         "Running Teleopti Detailed Export for dates {0} to {1}",
                                                         _startDateOnlyDto.DateTime, _endDateOnlyDto.DateTime));

                var count = payrollExport.PersonCollection.Count;
                PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture,
                                                         "Running Export for {0} persons in batches of {1}", count,
                                                         batchSize));

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var step = getIncreasePercentagePerBatch(count);
                var progress = 10;

                for (var i = 0; i < count; i = i + batchSize)
                {
                    var currentAgents = payrollExport.PersonCollection.Take(batchSize).ToArray();
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(payrollExport.TimeZoneId);

					PayrollExportFeedback.ReportProgress(progress, "Loading data...");
	                var detailedExportDtos = schedulingService.GetTeleoptiDetailedExportData(currentAgents, _startDateOnlyDto, _endDateOnlyDto,
	                                                                timeZoneInfo.Id);

	                foreach (var dto in detailedExportDtos)
	                {
		                _payrollDt.LoadDataRow(
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
					                dto.PayrollCode,
					                dto.Time
				                }, true);
	                }

					progress += step;
                }

                var builder = new StringBuilder();
                using (var stringWriter = new StringWriter(builder, CultureInfo.InvariantCulture))
                {
                    _payrollDt.WriteXml(stringWriter, XmlWriteMode.IgnoreSchema, false);
                    stringWriter.Flush();
                }
                var document = new XmlDocument();
                document.LoadXml(builder.ToString());

                Logger.Debug("Appending format to export");
                var result = FormatAppender.AppendFormat(document, "TeleoptiDetailedExportFormat.xml");
                Logger.Debug("Done with appending format to export");
                Logger.Debug("Done with payroll export");

                stopwatch.Stop();
                PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture, "The payroll export took {0} to complete.", stopwatch.Elapsed));

                return result;
            }
        }
		
		private void addColumnsToTable()
	    {
		    _payrollDt.Locale = CultureInfo.InvariantCulture;
		    _payrollDt.Columns.Add("Person", typeof (string));
		    _payrollDt.Columns.Add("FirstName", typeof (string));
		    _payrollDt.Columns.Add("LastName", typeof (string));
			_payrollDt.Columns.Add("BusinessUnitName", typeof (string));
			_payrollDt.Columns.Add("SiteName", typeof (string));
			_payrollDt.Columns.Add("TeamName", typeof (string));
			_payrollDt.Columns.Add("ContractName", typeof (string));
			_payrollDt.Columns.Add("PartTimePercentageName", typeof (string));
			_payrollDt.Columns.Add("PartTimePercentageNumber", typeof (double));
		    _payrollDt.Columns.Add("Date", typeof (DateTime));
		    _payrollDt.Columns.Add("PayrollCode", typeof (string));
		    _payrollDt.Columns.Add("Time", typeof (TimeSpan));
	    }

	    private static int getIncreasePercentagePerBatch(int count)
        {
            var batchCount = Math.Max(count / batchSize, 1);
            return 80 / batchCount;
        }

        public IPayrollExportFeedback PayrollExportFeedback { get; set; }
    }
}

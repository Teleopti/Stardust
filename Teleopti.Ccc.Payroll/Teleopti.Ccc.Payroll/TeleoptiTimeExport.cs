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
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using log4net;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Payroll
{
    public class TeleoptiTimeExport : IPayrollExportProcessorWithFeedback
    {
        private const int BatchSize = 50;
        private readonly static ILog Logger = LogManager.GetLogger(typeof (TeleoptiTimeExport));
        private static readonly PayrollFormatDto Format = new PayrollFormatDto(new Guid("{5A888BEC-5954-466d-B245-639BFEDA1BB5}"), "Teleopti Time Export");
        private DataTable payrollDt;

        public PayrollFormatDto PayrollFormat
        {
            get { return Format; }
        }

       

        public IXPathNavigable ProcessPayrollData(ITeleoptiSchedulingService schedulingService, ITeleoptiOrganizationService organizationService, PayrollExportDto payrollExport)
        {
            using(payrollDt = new DataTable("PayrollExport"))
            {

                AddColumnsToTable();
                
                IDictionary<Guid, PersonDto> personDic = new Dictionary<Guid, PersonDto>();
                foreach (PersonDto personDto in payrollExport.PersonCollection)
                {
                    if(personDto.Id != null)
                     personDic.Add(personDto.Id.Value, personDto);
                }

                DateOnlyDto startDateOnlyDto = payrollExport.DatePeriod.StartDate;
                DateOnlyDto endDateOnlyDto = payrollExport.DatePeriod.EndDate;

                PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture,
                                                         "Running Teleopti Time Export for dates {0} to {1}",
                                                         startDateOnlyDto.DateTime, endDateOnlyDto.DateTime));

                var count = payrollExport.PersonCollection.Count;
                PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture,
                                                         "Running Export for {0} persons in batches of {1}", count,
                                                         BatchSize));

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                int step = GetIncreasePercentagePerBatch(count);
                var progress = 10;

                for (int i = 0; i < count; i = i + BatchSize)
                {
                    PayrollExportFeedback.ReportProgress(progress, "Loading schedules...");

                    var currentAgents = payrollExport.PersonCollection.Take(BatchSize).ToArray();
                    var personTimeZone = TimeZoneInfo.FindSystemTimeZoneById(payrollExport.TimeZoneId);
                    
                    #pragma warning disable 612,618
                                        ICollection<PayrollBaseExportDto> payrollTimeExportDataList =
                                            schedulingService.GetTeleoptiTimeExportData(currentAgents,
                    #pragma warning restore 612,618
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
                PayrollExportFeedback.Info(string.Format(CultureInfo.InvariantCulture,"The payroll export took {0} to complete.",stopwatch.Elapsed));

                return result;
            }
        }

        private void AddColumnsToTable()
        {
            payrollDt.Locale = CultureInfo.InvariantCulture;
            payrollDt.Columns.Add("EmploymentNumber", typeof(string));
            payrollDt.Columns.Add("FirstName", typeof(string));
            payrollDt.Columns.Add("LastName", typeof(string));
            payrollDt.Columns.Add("BusinessUnitName", typeof(string));
            payrollDt.Columns.Add("SiteName", typeof(string));
            payrollDt.Columns.Add("TeamName", typeof(string));
            payrollDt.Columns.Add("ContractName", typeof(string));
            payrollDt.Columns.Add("PartTimePercentageName", typeof(string));
            payrollDt.Columns.Add("Date", typeof(string));
            payrollDt.Columns.Add("StartDate", typeof(string));
            payrollDt.Columns.Add("EndDate", typeof(string));
            payrollDt.Columns.Add("ShiftCategoryName", typeof(string));
            payrollDt.Columns.Add("ContractTime", typeof(string));
            payrollDt.Columns.Add("WorkTime", typeof(string));
            payrollDt.Columns.Add("PaidTime", typeof(string));
            payrollDt.Columns.Add("AbsencePayrollCode", typeof(string));
            payrollDt.Columns.Add("DayOffPayrollCode", typeof(string));


        }

        private static int GetIncreasePercentagePerBatch(int count)
        {
            var batchCount = Math.Max(count/BatchSize,1);
            return 80/batchCount;
        }

        public IPayrollExportFeedback PayrollExportFeedback { get; set; }
    }
}

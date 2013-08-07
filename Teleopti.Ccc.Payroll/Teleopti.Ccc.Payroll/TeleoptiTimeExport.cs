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
    public class TeleoptiTimeExport : IPayrollExportProcessorWithFeedback
    {
        private const int BatchSize = 50;
        private readonly static ILog Logger = LogManager.GetLogger(typeof (TeleoptiTimeExport));
        private static readonly PayrollFormatDto Format = new PayrollFormatDto(new Guid("{5A888BEC-5954-466d-B245-639BFEDA1BB5}"), "Teleopti Time Export");

        public PayrollFormatDto PayrollFormat
        {
            get { return Format; }
        }

        public IXPathNavigable ProcessPayrollData(ITeleoptiSchedulingService schedulingService, ITeleoptiOrganizationService organizationService, PayrollExportDto payrollExport)
        {
            using(var payrollDt = new DataTable("PayrollExport"))
            {
                payrollDt.Locale = CultureInfo.InvariantCulture;
                payrollDt.Columns.Add("Person", typeof(string));
                payrollDt.Columns.Add("Date", typeof(DateTime));
                payrollDt.Columns.Add("Time", typeof(TimeSpan));

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

                    var currentAgents = payrollExport.PersonCollection.Skip(i).Take(BatchSize).ToArray();
                    var personTimeZone = TimeZoneInfo.FindSystemTimeZoneById(payrollExport.TimeZoneId);
#pragma warning disable 612,618
                    ICollection<SchedulePartDto> scheduleParts =
                        schedulingService.GetSchedulePartsForPersons(currentAgents,
#pragma warning restore 612,618
                                                                     startDateOnlyDto,
                                                                     endDateOnlyDto,
                                                                     personTimeZone.Id);

                    foreach (SchedulePartDto schedulePart in scheduleParts)
                    {
                        var person = personDic[schedulePart.PersonId];
                        var firstPeriod = person.PersonPeriodCollection.OrderBy(p => p.Period.StartDate.DateTime).FirstOrDefault();
                        if (firstPeriod != null && schedulePart.Date.DateTime < firstPeriod.Period.StartDate.DateTime)
                            continue;
                        var teminateDate = person.TerminationDate;
                        if (teminateDate != null && schedulePart.Date.DateTime > teminateDate.DateTime)
                            continue;
                        if (person.TimeZoneId != personTimeZone.Id)
                            personTimeZone = TimeZoneInfo.FindSystemTimeZoneById(person.TimeZoneId);
                        payrollDt.LoadDataRow(
                            new object[]
                            {
                                personDic[schedulePart.PersonId].EmploymentNumber,
                                TimeZoneInfo.ConvertTimeFromUtc(schedulePart.LocalPeriod.UtcStartTime, personTimeZone),
                                schedulePart.ContractTime.TimeOfDay
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

        private static int GetIncreasePercentagePerBatch(int count)
        {
            var batchCount = Math.Max(count/BatchSize,1);
            return 80/batchCount;
        }

        public IPayrollExportFeedback PayrollExportFeedback { get; set; }
    }
}

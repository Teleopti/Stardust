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
        private IDictionary<Guid, AbsenceDto> _absenceDic;
        private IDictionary<Guid, PersonDto> _personDic;
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
                prepareAbsenceCache(schedulingService);
                preparePeopleCache(payrollExport);
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

                int step = getIncreasePercentagePerBatch(count);
                var progress = 10;

                for (int i = 0; i < count; i = i + batchSize)
                {
                    var currentAgents = payrollExport.PersonCollection.Take(batchSize).ToArray();
                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(payrollExport.TimeZoneId);

                    PayrollExportFeedback.ReportProgress(progress, "Loading schedules...");
                    exportPersonAbsences(schedulingService, currentAgents, timeZoneInfo);

                    progress += step;

                    PayrollExportFeedback.ReportProgress(progress, "Loading multiplicator data...");
                    exportMultiplicatorData(schedulingService, currentAgents, timeZoneInfo);

                    progress += step;
                }

                var builder = new StringBuilder();
                using (
                    var stringWriter = new StringWriter(builder, CultureInfo.InvariantCulture)
                    )
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

        private void exportMultiplicatorData(ITeleoptiSchedulingService schedulingService, PersonDto[] currentAgents, TimeZoneInfo timeZoneInfo)
        {
            var multiplicatorData = schedulingService.GetPersonMultiplicatorDataForPersons(currentAgents,
                                                                                           _startDateOnlyDto,
                                                                                           _endDateOnlyDto,
                                                                                           timeZoneInfo.Id);
            foreach (var multiplicatorDataDto in multiplicatorData)
            {
                var person = _personDic[multiplicatorDataDto.PersonId.GetValueOrDefault(Guid.Empty)];
                var teminateDate = person.TerminationDate;
                if (teminateDate != null && multiplicatorDataDto.Date > teminateDate.DateTime)
                    continue;
                _payrollDt.LoadDataRow(
                    new object[]
                        {
                            person.EmploymentNumber,
                            multiplicatorDataDto.Date,
                            multiplicatorDataDto.Multiplicator.PayrollCode,
                            multiplicatorDataDto.Amount
                        },	true);
            }
        }

        private void exportPersonAbsences(ITeleoptiSchedulingService schedulingService, PersonDto[] currentAgents, TimeZoneInfo timeZoneInfo)
        {
#pragma warning disable 612,618
            ICollection<SchedulePartDto> scheduleParts = schedulingService.GetSchedulePartsForPersons(currentAgents,
#pragma warning restore 612,618
                                                                                                      _startDateOnlyDto,
                                                                                                      _endDateOnlyDto,
                                                                                                      timeZoneInfo.Id);

            var personTimeZone = timeZoneInfo;
            foreach (SchedulePartDto schedulePart in scheduleParts)
            {
                var person = _personDic[schedulePart.PersonId];
                var firstPeriod = person.PersonPeriodCollection.OrderBy(p => p.Period.StartDate.DateTime).FirstOrDefault();
                if (firstPeriod != null && schedulePart.Date.DateTime < firstPeriod.Period.StartDate.DateTime)
                    continue;
                var teminateDate = person.TerminationDate;
                if (teminateDate != null && schedulePart.Date.DateTime > teminateDate.DateTime)
                    continue;
                foreach (ProjectedLayerDto layerDto in schedulePart.ProjectedLayerCollection)
                {
                    if (layerDto.IsAbsence && layerDto.ContractTime != TimeSpan.Zero)
                    {
                        if (person.TimeZoneId != personTimeZone.Id)
                            personTimeZone = TimeZoneInfo.FindSystemTimeZoneById(person.TimeZoneId);
	                    _payrollDt.LoadDataRow(
		                    new object[]
			                    {
				                    person.EmploymentNumber,
				                    TimeZoneInfo.ConvertTimeFromUtc(layerDto.Period.UtcStartTime, personTimeZone),
				                    _absenceDic[layerDto.PayloadId].PayrollCode,
				                    layerDto.ContractTime
			                    }, true);
                    }
                }
            }
        }

        private void preparePeopleCache(PayrollExportDto payrollExport)
        {
            _personDic = new Dictionary<Guid, PersonDto>();
            foreach (PersonDto personDto in payrollExport.PersonCollection)
            {
                if (personDto.Id.HasValue)
                    _personDic.Add(personDto.Id.Value, personDto);
            }
        }

        private void prepareAbsenceCache(ITeleoptiSchedulingService schedulingService)
        {
	        var absences =
		        schedulingService.GetAbsences(new AbsenceLoadOptionDto {LoadDeleted = true});
            _absenceDic = new Dictionary<Guid, AbsenceDto>();
            foreach (AbsenceDto dto in absences)
            {
                if(dto.Id.HasValue)
                    _absenceDic.Add(dto.Id.Value, dto);
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
			_payrollDt.Columns.Add("PartTimePercentageNumber", typeof (decimal));
		    _payrollDt.Columns.Add("Date", typeof (DateTime));
		    _payrollDt.Columns.Add("PayrollCode", typeof (string));
		    _payrollDt.Columns.Add("Time", typeof (TimeSpan));
	    }

	    private static int getIncreasePercentagePerBatch(int count)
        {
            var batchCount = Math.Max(count / batchSize, 1) * 2;
            return 80 / batchCount;
        }

        public IPayrollExportFeedback PayrollExportFeedback { get; set; }
    }
}

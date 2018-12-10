using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using DateOnly = Teleopti.Interfaces.Domain.DateOnly;
using DateOnlyPeriod = Teleopti.Interfaces.Domain.DateOnlyPeriod;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollDataExtractor : IPayrollDataExtractor
	{
		private readonly IPlugInLoader _plugInLoader;
		private readonly IChannelCreator _channelCreator;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(PayrollDataExtractor));

		public PayrollDataExtractor(IPlugInLoader plugInLoader, IChannelCreator channelCreator)
		{
			_plugInLoader = plugInLoader;
			_channelCreator = channelCreator;
		}

		public IXPathNavigable Extract(IPayrollExport payrollExport, RunPayrollExportEvent message,
			IEnumerable<PersonDto> personDtos, IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback)
		{
			var availablePayrollExportProcessors = _plugInLoader.Load();

			var selectedProcessor =
				availablePayrollExportProcessors.FirstOrDefault(p => p.PayrollFormat.FormatId == payrollExport.PayrollFormatId);			

			if (selectedProcessor == null)
			{
				var processorNotFoundMessage = $"The selected payroll export processor was not found. (PayrollFormatId = {payrollExport.PayrollFormatId})";
				Logger.ErrorFormat(processorNotFoundMessage);
				var userErrorMessage = "Payroll export failed. The selected payroll export processor was not found.";
				serviceBusPayrollExportFeedback.Error(userErrorMessage);
				return null;
			}

			var payrollExportProcessorWithFeedback = selectedProcessor as IPayrollExportProcessorWithFeedback;
			if (payrollExportProcessorWithFeedback != null)
			{
				Logger.Debug("The selected payroll export processor can report progress and other feedback.");
				payrollExportProcessorWithFeedback.PayrollExportFeedback = serviceBusPayrollExportFeedback;
			}

			var payrollExportDto = CreateDto(payrollExport, message, personDtos);

			var result = selectedProcessor.ProcessPayrollData(_channelCreator.CreateChannel<ITeleoptiSchedulingService>(),
				_channelCreator.CreateChannel<ITeleoptiOrganizationService>(),
				payrollExportDto);

			if (payrollExportProcessorWithFeedback != null)
			{
				payrollExportProcessorWithFeedback.PayrollExportFeedback = null;
			}

			return result;
		}

		private static PayrollExportDto CreateDto(IPayrollExport payrollExport, RunPayrollExportEvent @event, IEnumerable<PersonDto> personDtos)
		{
			var timeZone = payrollExport.CreatedBy.PermissionInformation.DefaultTimeZone();
			var origPeriod = new DateOnlyPeriod(new DateOnly(@event.ExportStartDate), new DateOnly(@event.ExportEndDate));
			var period = origPeriod.ToDateTimePeriod(timeZone);
			var exportDto = new PayrollExportDto
			{
				DatePeriod =
					new DateOnlyPeriodDto
					{
						StartDate = new DateOnlyDto { DateTime = @event.ExportStartDate },
						EndDate = new DateOnlyDto { DateTime = @event.ExportEndDate }
					},
				Id = payrollExport.Id,
				PayrollFormat = new PayrollFormatDto(@event.PayrollExportFormatId, string.Empty),
				Period =
					new DateTimePeriodDto
					{
						UtcStartTime = period.StartDateTime,
						UtcEndTime = period.EndDateTime,
						LocalStartDateTime = period.StartDateTimeLocal(timeZone),
						LocalEndDateTime = period.EndDateTimeLocal(timeZone)
					},
				TimeZoneId = timeZone.Id
			};
			personDtos.ForEach(exportDto.PersonCollection.Add);

			return exportDto;
		}
	}
}
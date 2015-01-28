using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Teleopti.Interfaces.Messages.Payroll;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
	public class PayrollDataExtractor : IPayrollDataExtractor
	{
		private readonly IPlugInLoader _plugInLoader;
		private readonly IChannelCreator _channelCreator;
		private readonly static ILog Logger = LogManager.GetLogger(typeof(PayrollExportConsumer));

		public PayrollDataExtractor(IPlugInLoader plugInLoader, IChannelCreator channelCreator)
		{
			_plugInLoader = plugInLoader;
			_channelCreator = channelCreator;
		}

		public IXPathNavigable Extract(IPayrollExport payrollExport, RunPayrollExport message, IEnumerable<PersonDto> personDtos, IServiceBusPayrollExportFeedback serviceBusPayrollExportFeedback)
		{
			var payrollExportProcessors = _plugInLoader.Load();
			var selectedProcessor = payrollExportProcessors.FirstOrDefault(p => p.PayrollFormat.FormatId == payrollExport.PayrollFormatId);

			if (selectedProcessor == null)
			{
				Logger.ErrorFormat("The selected payroll export processor was not found in the bus. (PayrollFormatId = {0})", payrollExport.PayrollFormatId);
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

		private static PayrollExportDto CreateDto(IPayrollExport payrollExport, RunPayrollExport message, IEnumerable<PersonDto> personDtos)
		{
			var timeZone = payrollExport.CreatedBy.PermissionInformation.DefaultTimeZone();
			var period =
				 message.ExportPeriod.ToDateTimePeriod(timeZone);
			var exportDto = new PayrollExportDto
									  {
										  DatePeriod = new DateOnlyPeriodDto { StartDate = new DateOnlyDto { DateTime = message.ExportPeriod.StartDate }, EndDate = new DateOnlyDto { DateTime = message.ExportPeriod.EndDate } },
										  Id = payrollExport.Id,
										  PayrollFormat = new PayrollFormatDto(message.PayrollExportFormatId, string.Empty),
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
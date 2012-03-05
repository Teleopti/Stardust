using System.ServiceModel;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.WcfService.Factory;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.WcfService.CommandHandler
{
    public class ImportForecastsFileCommandHandler : IHandleCommand<ImportForecastsFileCommandDto>
    {
        private readonly IServiceBusSender _busSender;

        public ImportForecastsFileCommandHandler(IServiceBusSender busSender)
        {
            _busSender = busSender;
        }

        public CommandResultDto Handle(ImportForecastsFileCommandDto command)
        {
            if (!_busSender.EnsureBus())
            {
                throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the import forecasts.");
            }
            var message = new ImportForecastsFileToSkill {JobId = command.UploadedFileId};
            _busSender.NotifyServiceBus(message);
            return new CommandResultDto { AffectedId = command.UploadedFileId, AffectedItems = 1 };
        }
    }
}

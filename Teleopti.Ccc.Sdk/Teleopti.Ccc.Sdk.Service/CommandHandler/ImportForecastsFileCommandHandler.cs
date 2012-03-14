using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
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
            var identity = (TeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
            var message = new ImportForecastsFileToSkill
            {
                JobId = command.UploadedFileId,
                TargetSkillId = command.TargetSkillId,
                OwnerPersonId = ((IUnsafePerson)TeleoptiPrincipal.Current).Person.Id.GetValueOrDefault(Guid.Empty),
                BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
                Datasource = identity.DataSource.Application.Name,
                Timestamp = DateTime.UtcNow,
                ImportMode = getImportForecastMode(command.ImportForecastsMode)
            };
            _busSender.NotifyServiceBus(message);
            return new CommandResultDto { AffectedId = command.UploadedFileId, AffectedItems = 1 };
        }

        private static ImportForecastsMode getImportForecastMode(ImportForecastsOptionsDto optionsDto)
        {
            switch (optionsDto)
            {
                case ImportForecastsOptionsDto.ImportWorkload:
                    return ImportForecastsMode.ImportWorkload;
                case ImportForecastsOptionsDto.ImportStaffing:
                    return ImportForecastsMode.ImportStaffing;
                case ImportForecastsOptionsDto.ImportWorkloadAndStaffing:
                    return ImportForecastsMode.ImportWorkloadAndStaffing;
            }
            return ImportForecastsMode.ImportWorkloadAndStaffing;
        }
    }
}

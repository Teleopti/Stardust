﻿using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ImportForecastsFileCommandHandler : IHandleCommand<ImportForecastsFileCommandDto>
    {
        private readonly IServiceBusSender _busSender;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IJobResultRepository _jobResultRepository;

        public ImportForecastsFileCommandHandler(IServiceBusSender busSender,
            IUnitOfWorkFactory unitOfWorkFactory,
            IJobResultRepository jobResultRepository)
        {
            _busSender = busSender;
            _unitOfWorkFactory = unitOfWorkFactory;
            _jobResultRepository = jobResultRepository;
        }

        public void Handle(ImportForecastsFileCommandDto command)
        {
            if (command == null)
                throw new FaultException("Command is null.");
            if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ImportForecastFromFile))
            {
                throw new FaultException("You're not authorized to run this command.");
            }
            Guid jobResultId;
            var person = ((IUnsafePerson) TeleoptiPrincipal.Current).Person;
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
                                              person, DateTime.UtcNow);
                _jobResultRepository.Add(jobResult);
                jobResultId = jobResult.Id.GetValueOrDefault();
                unitOfWork.PersistAll();
            }
            
            if (!_busSender.EnsureBus())
            {
                throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the import forecasts.");
            }
            var identity = (ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
            var message = new ImportForecastsFileToSkill
            {
                JobId = jobResultId,
                UploadedFileId = command.UploadedFileId,
                TargetSkillId = command.TargetSkillId,
                OwnerPersonId = person.Id.GetValueOrDefault(Guid.Empty),
                BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
                Datasource = identity.DataSource.Application.Name,
                Timestamp = DateTime.UtcNow,
                ImportMode = (ImportForecastsMode)((int)command.ImportForecastsMode)
            };
            _busSender.Send(message);
			command.Result = new CommandResultDto { AffectedId = jobResultId, AffectedItems = 1 };
        }
    }
}

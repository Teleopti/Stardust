using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ImportForecastsFileCommandHandler : IHandleCommand<ImportForecastsFileCommandDto>
    {
		private readonly IMessagePopulatingServiceBusSender _busSender;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IJobResultRepository _jobResultRepository;

		public ImportForecastsFileCommandHandler(IMessagePopulatingServiceBusSender busSender,
            ICurrentUnitOfWorkFactory unitOfWorkFactory,
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
            var person = ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person;
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
                                              person, DateTime.UtcNow);
                _jobResultRepository.Add(jobResult);
                jobResultId = jobResult.Id.GetValueOrDefault();
                unitOfWork.PersistAll();
            }
            
            var message = new ImportForecastsFileToSkill
            {
                JobId = jobResultId,
                UploadedFileId = command.UploadedFileId,
                TargetSkillId = command.TargetSkillId,
                OwnerPersonId = person.Id.GetValueOrDefault(Guid.Empty),
                ImportMode = (ImportForecastsMode)((int)command.ImportForecastsMode)
            };
			_busSender.Send(message, true);
			command.Result = new CommandResultDto { AffectedId = jobResultId, AffectedItems = 1 };
        }
    }
}

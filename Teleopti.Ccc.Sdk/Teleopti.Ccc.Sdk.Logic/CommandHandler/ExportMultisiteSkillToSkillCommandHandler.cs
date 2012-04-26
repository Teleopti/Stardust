using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ExportMultisiteSkillToSkillCommandHandler : IHandleCommand<ExportMultisiteSkillToSkillCommandDto>
    {
        private readonly IServiceBusSender _busSender;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

        public ExportMultisiteSkillToSkillCommandHandler(IServiceBusSender busSender, IUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository)
        {
            _busSender = busSender;
            _unitOfWorkFactory = unitOfWorkFactory;
            _jobResultRepository = jobResultRepository;
        }

        public CommandResultDto Handle(ExportMultisiteSkillToSkillCommandDto command)
        {
			if (!TeleoptiPrincipal.Current.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit))
			{
				throw new FaultException("You're not authorized to run this command.");
			}

            if (!_busSender.EnsureBus())
            {
                throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the export.");
            }

            Guid jobId;
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                //Save start of processing to job history
                var period = new DateOnlyPeriod(new DateOnly(command.Period.StartDate.DateTime),
                                                new DateOnly(command.Period.EndDate.DateTime));
                var jobResult = new JobResult(JobCategory.MultisiteExport, period,
                                              ((IUnsafePerson) TeleoptiPrincipal.Current).Person, DateTime.UtcNow);
                _jobResultRepository.Add(jobResult);
                jobId = jobResult.Id.GetValueOrDefault();
                unitOfWork.PersistAll();

                //Prepare message to send to service bus
                var identity = (TeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
                var message = new ExportMultisiteSkillsToSkill
                                  {
                                      BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
                                      Datasource = identity.DataSource.Application.Name,
                                      Timestamp = DateTime.UtcNow,
                                      OwnerPersonId =
                                          ((IUnsafePerson) TeleoptiPrincipal.Current).Person.Id.GetValueOrDefault(
                                              Guid.Empty),
                                              JobId = jobId,
                                      Period = period
                                  };
                foreach (var multisiteSkillSelection in command.MultisiteSkillSelection)
                {
                    var selection = new MultisiteSkillSelection();
                    selection.MultisiteSkillId = multisiteSkillSelection.MultisiteSkill.Id.GetValueOrDefault();

                    foreach (var childSkillMappingDto in multisiteSkillSelection.ChildSkillMapping)
                    {
                        var childSkillMapping = new ChildSkillSelection();
                        childSkillMapping.SourceSkillId = childSkillMappingDto.SourceSkill.Id.GetValueOrDefault();
                        childSkillMapping.TargetSkillId = childSkillMappingDto.TargetSkill.Id.GetValueOrDefault();
                        selection.ChildSkillSelections.Add(childSkillMapping);
                    }
					message.MultisiteSkillSelections.Add(selection);
                }

                _busSender.NotifyServiceBus(message);
            }
            return new CommandResultDto {AffectedId = jobId, AffectedItems = 1};
        }
    }
}

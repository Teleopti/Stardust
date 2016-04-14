using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ExportMultisiteSkillToSkillCommandHandler : IHandleCommand<ExportMultisiteSkillToSkillCommandDto>
    {
		private readonly IMessagePopulatingServiceBusSender _busSender;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

		public ExportMultisiteSkillToSkillCommandHandler(IMessagePopulatingServiceBusSender busSender, ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository)
        {
            _busSender = busSender;
            _unitOfWorkFactory = unitOfWorkFactory;
            _jobResultRepository = jobResultRepository;
        }

		public void Handle(ExportMultisiteSkillToSkillCommandDto command)
        {
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit))
			{
				throw new FaultException("You're not authorized to run this command.");
			}

            Guid jobId;
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                //Save start of processing to job history
                var period = command.Period.ToDateOnlyPeriod();
                var jobResult = new JobResult(JobCategory.MultisiteExport, period,
                                              ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person, DateTime.UtcNow);
                _jobResultRepository.Add(jobResult);
                jobId = jobResult.Id.GetValueOrDefault();
                unitOfWork.PersistAll();

                //Prepare message to send to service bus
                var message = new ExportMultisiteSkillsToSkill
                                  {
                                      OwnerPersonId =
                                          ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person.Id.GetValueOrDefault(
                                              Guid.Empty),
                                              JobId = jobId,
                                      PeriodStart = period.StartDate.Date,
									  PeriodEnd = period.EndDate.Date
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

				_busSender.Send(message, true);
            }
			command.Result = new CommandResultDto { AffectedId = jobId, AffectedItems = 1 };
        }
    }
}

using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ExportMultisiteSkillToSkillCommandHandler : IHandleCommand<ExportMultisiteSkillToSkillCommandDto>
    {
		private readonly IEventPublisher _publisher;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
	    private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly ILoggedOnUser _loggedOnUser;

		public ExportMultisiteSkillToSkillCommandHandler(IEventPublisher publisher, ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator, ILoggedOnUser loggedOnUser)
        {
            _publisher = publisher;
            _unitOfWorkFactory = unitOfWorkFactory;
            _jobResultRepository = jobResultRepository;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_loggedOnUser = loggedOnUser;
		}

		public void Handle(ExportMultisiteSkillToSkillCommandDto command)
        {
			if (!PrincipalAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit))
			{
				throw new FaultException("You're not authorized to run this command.");
			}

            Guid jobId;
            using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                //Save start of processing to job history
                var period = command.Period.ToDateOnlyPeriod();
                var jobResult = new JobResult(JobCategory.MultisiteExport, period, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
                _jobResultRepository.Add(jobResult);
                jobId = jobResult.Id.GetValueOrDefault();
                unitOfWork.PersistAll();

                //Prepare message to send to service bus
                var message = new ExportMultisiteSkillsToSkillEvent
                                  {
                                      OwnerPersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault(Guid.Empty),
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
				_eventInfrastructureInfoPopulator.PopulateEventContext(message);
				_publisher.Publish(message);
            }
			command.Result = new CommandResultDto { AffectedId = jobId, AffectedItems = 1 };
        }
    }
}

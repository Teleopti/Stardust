﻿using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ExportMultisiteSkillToSkillCommandHandler : IHandleCommand<ExportMultisiteSkillToSkillCommandDto>
    {
		private readonly IEventPublisher _publisher;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
	    private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;

		public ExportMultisiteSkillToSkillCommandHandler(IEventPublisher publisher, ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator)
        {
            _publisher = publisher;
            _unitOfWorkFactory = unitOfWorkFactory;
            _jobResultRepository = jobResultRepository;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
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
                var jobResult = new JobResult(JobCategory.MultisiteExport, period,
                                              ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person, DateTime.UtcNow);
                _jobResultRepository.Add(jobResult);
                jobId = jobResult.Id.GetValueOrDefault();
                unitOfWork.PersistAll();

                //Prepare message to send to service bus
                var message = new ExportMultisiteSkillsToSkillEvent
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
				_eventInfrastructureInfoPopulator.PopulateEventContext(message);
				_publisher.Publish(message);
            }
			command.Result = new CommandResultDto { AffectedId = jobId, AffectedItems = 1 };
        }
    }
}

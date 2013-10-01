using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
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
        private readonly IServiceBusSender _busSender;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

        public ExportMultisiteSkillToSkillCommandHandler(IServiceBusSender busSender, ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository)
        {
            _busSender = busSender;
            _unitOfWorkFactory = unitOfWorkFactory;
            _jobResultRepository = jobResultRepository;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Handle(ExportMultisiteSkillToSkillCommandDto command)
        {
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.ExportForecastToOtherBusinessUnit))
			{
				throw new FaultException("You're not authorized to run this command.");
			}

            if (!_busSender.EnsureBus())
            {
                throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the export.");
            }

            Guid jobId;
            using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
                //Save start of processing to job history
                var period = command.Period.ToDateOnlyPeriod();
                var jobResult = new JobResult(JobCategory.MultisiteExport, period,
                                              ((IUnsafePerson) TeleoptiPrincipal.Current).Person, DateTime.UtcNow);
                _jobResultRepository.Add(jobResult);
                jobId = jobResult.Id.GetValueOrDefault();
                unitOfWork.PersistAll();

                //Prepare message to send to service bus
                var message = new ExportMultisiteSkillsToSkill
                                  {
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

                _busSender.Send(message);
            }
			command.Result = new CommandResultDto { AffectedId = jobId, AffectedItems = 1 };
        }
    }
}

using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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
	public class QuickForecastCommandHandler : IHandleCommand<QuickForecastCommandDto>
    {
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
	    private readonly IEventPublisher _publisher;
	    private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;

	    public QuickForecastCommandHandler(IMessagePopulatingServiceBusSender busSender, ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository, IEventPublisher publisher, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
	        _publisher = publisher;
	        _eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
		}

		public void Handle(QuickForecastCommandDto command)
		{
			if (command == null)
				throw new FaultException("Command is null.");
			Guid jobId;

			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				//Save start of processing to job history
				var period = command.TargetPeriod.ToDateOnlyPeriod();
				var jobResult = new JobResult(JobCategory.QuickForecast, period,
				                              ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person, DateTime.UtcNow);
				_jobResultRepository.Add(jobResult);
				jobId = jobResult.Id.GetValueOrDefault();

				unitOfWork.PersistAll();

				var message = new QuickForecastWorkloadsEvent
					{
						TargetPeriodStart = command.TargetPeriod.StartDate.ToDateOnly(),
                        TargetPeriodEnd = command.TargetPeriod.EndDate.ToDateOnly(),
						ScenarioId = command.ScenarioId,
						JobId = jobId,
						SmoothingStyle = command.SmoothingStyle,
						TemplatePeriodStart = command.TemplatePeriod.StartDate.ToDateOnly(),
                        TemplatePeriodEnd = command.TemplatePeriod.EndDate.ToDateOnly(),
						WorkloadIds = command.WorkloadIds,
						IncreaseWith = command.IncreaseWith,
                        UseDayOfMonth = command.UseDayOfMonth,
                        StatisticsPeriodStart = command.StatisticPeriod.StartDate.ToDateOnly(),
                        StatisticsPeriodEnd = command.StatisticPeriod.EndDate.ToDateOnly()
					};

                _eventInfrastructureInfoPopulator.PopulateEventContext(message);
                _publisher.Publish(message);
            }

			command.Result = new CommandResultDto {AffectedId = jobId, AffectedItems = 1};
		}
    }
}

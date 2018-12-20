using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class QuickForecastCommandHandler : IHandleCommand<QuickForecastCommandDto>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPublisher _publisher;
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly ILoggedOnUser _loggedOnUser;

		public QuickForecastCommandHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository, IEventPublisher publisher, IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator, ILoggedOnUser loggedOnUser)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_publisher = publisher;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_loggedOnUser = loggedOnUser;
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
				var jobResult = new JobResult(JobCategory.QuickForecast, period, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
				_jobResultRepository.Add(jobResult);
				jobId = jobResult.Id.GetValueOrDefault();

				unitOfWork.PersistAll();

				var message = new QuickForecastWorkloadsEvent
					{
						TargetPeriodStart = command.TargetPeriod.StartDate.DateTime,
						TargetPeriodEnd = command.TargetPeriod.EndDate.DateTime,
						ScenarioId = command.ScenarioId,
						JobId = jobId,
						SmoothingStyle = command.SmoothingStyle,
						TemplatePeriodStart = command.TemplatePeriod.StartDate.DateTime,
						TemplatePeriodEnd = command.TemplatePeriod.EndDate.DateTime,
						WorkloadIds = command.WorkloadIds,
						IncreaseWith = command.IncreaseWith,
						UseDayOfMonth = command.UseDayOfMonth,
						StatisticPeriodStart = command.StatisticPeriod.StartDate.DateTime,
						StatisticPeriodEnd = command.StatisticPeriod.EndDate.DateTime
                };

				_eventInfrastructureInfoPopulator.PopulateEventContext(message);
				_publisher.Publish(message);
			}

			command.Result = new CommandResultDto {AffectedId = jobId, AffectedItems = 1};
		}
	}
}

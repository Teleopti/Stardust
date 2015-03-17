using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class QuickForecastCommandHandler : IHandleCommand<QuickForecastCommandDto>
    {
		private readonly IMessagePopulatingServiceBusSender _busSender;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

		public QuickForecastCommandHandler(IMessagePopulatingServiceBusSender busSender, ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository)
		{
			_busSender = busSender;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
		}

		public void Handle(QuickForecastCommandDto command)
		{
			if (command == null)
				throw new FaultException("Command is null.");
			Guid jobId;

			using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				//Save start of processing to job history
				var period = command.TargetPeriod.ToDateOnlyPeriod();
				var jobResult = new JobResult(JobCategory.QuickForecast, period,
				                              ((IUnsafePerson) TeleoptiPrincipal.CurrentPrincipal).Person, DateTime.UtcNow);
				_jobResultRepository.Add(jobResult);
				jobId = jobResult.Id.GetValueOrDefault();

				unitOfWork.PersistAll();

				var message = new QuickForecastWorkloadsMessage
					{
						StatisticPeriod = command.StatisticPeriod.ToDateOnlyPeriod(),
						TargetPeriod = command.TargetPeriod.ToDateOnlyPeriod(),
						ScenarioId = command.ScenarioId,
						JobId = jobId,
						SmoothingStyle = command.SmoothingStyle,
						TemplatePeriod = command.TemplatePeriod.ToDateOnlyPeriod(),
						WorkloadIds = command.WorkloadIds,
						IncreaseWith = command.IncreaseWith,
                        UseDayOfMonth = command.UseDayOfMonth
					};

				_busSender.Send(message, true);
			}

			command.Result = new CommandResultDto {AffectedId = jobId, AffectedItems = 1};
		}
    }
}

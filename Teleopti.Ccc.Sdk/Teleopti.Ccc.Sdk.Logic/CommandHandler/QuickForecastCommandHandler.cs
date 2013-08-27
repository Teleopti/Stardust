using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class QuickForecastCommandHandler : IHandleCommand<QuickForecastCommandDto>
    {
    	private readonly IServiceBusSender _busSender;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

		public QuickForecastCommandHandler(IServiceBusSender busSender, ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository)
		{
			_busSender = busSender;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(QuickForecastCommandDto command)
		{
			if (command == null)
				throw new FaultException("Command is null.");
			Guid jobId;

			using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				//Save start of processing to job history
				var period = new DateOnlyPeriod(new DateOnly(command.TargetPeriod.StartDate.DateTime),
				                                new DateOnly(command.TargetPeriod.EndDate.DateTime));
				var jobResult = new JobResult(JobCategory.QuickForecast, period,
				                              ((IUnsafePerson) TeleoptiPrincipal.Current).Person, DateTime.UtcNow);
				_jobResultRepository.Add(jobResult);
				jobId = jobResult.Id.GetValueOrDefault();

				unitOfWork.PersistAll();
				if (!_busSender.EnsureBus())
				{
					throw new FaultException(
						"The outgoing queue for the service bus is not available. Cannot continue with the denormalizer.");
				}

				var message = new QuickForecastWorkloadsMessage
					{
						StatisticPeriod =
							new DateOnlyPeriod(new DateOnly(command.StatisticPeriod.StartDate.DateTime),
							                   new DateOnly(command.StatisticPeriod.EndDate.DateTime)),
						TargetPeriod =
							new DateOnlyPeriod(new DateOnly(command.TargetPeriod.StartDate.DateTime),
							                   new DateOnly(command.TargetPeriod.EndDate.DateTime)),
						ScenarioId = command.ScenarioId,
						JobId = jobId,
						SmoothingStyle = command.SmoothingStyle,
						TemplatePeriod =
							new DateOnlyPeriod(new DateOnly(command.TemplatePeriod.StartDate.DateTime),
							                   new DateOnly(command.TemplatePeriod.EndDate.DateTime)),
						WorkloadIds = command.WorkloadIds,
						IncreaseWith = command.IncreaseWith
					};
				message.SetMessageDetail();

				_busSender.Send(message);
			}

			command.Result = new CommandResultDto {AffectedId = jobId, AffectedItems = 1};
		}
    }
}

using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class QuickForecastCommandHandler : IHandleCommand<QuickForecastCommandDto>
    {
    	private readonly IServiceBusSender _busSender;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IJobResultRepository _jobResultRepository;

		public QuickForecastCommandHandler(IServiceBusSender busSender, IUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository)
		{
			_busSender = busSender;
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CommandResultDto Handle(QuickForecastCommandDto command)
		{
			if (!_busSender.EnsureBus())
			{
				throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the denormalizer.");
			}

			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				//Save start of processing to job history
                var period = new DateOnlyPeriod(new DateOnly(command.TargetPeriod.StartDate.DateTime),
                                                new DateOnly(command.TargetPeriod.EndDate.DateTime));
                var jobResult = new JobResult(JobCategory.QuickForecast, period,
                                              ((IUnsafePerson) TeleoptiPrincipal.Current).Person, DateTime.UtcNow);
                _jobResultRepository.Add(jobResult);
                var jobId = jobResult.Id.GetValueOrDefault();
                unitOfWork.PersistAll();

				var identity = (ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity;
				var message = new QuickForecastWorkloadsMessage
			              	{
			              		BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
			              		Datasource = identity.DataSource.Application.Name,
			              		Timestamp = DateTime.UtcNow,
								StatisticPeriod = new DateOnlyPeriod(new DateOnly(command.StatisticPeriod.StartDate.DateTime), new DateOnly(command.StatisticPeriod.EndDate.DateTime)),
								TargetPeriod = new DateOnlyPeriod(new DateOnly(command.TargetPeriod.StartDate.DateTime), new DateOnly(command.TargetPeriod.EndDate.DateTime)),
			              		ScenarioId = command.ScenarioId,
								JobId = jobId,
								UpdateStandardTemplates = command.UpdateStandardTemplates,
								WorkloadIds = command.WorkloadIds
			              	};

			_busSender.NotifyServiceBus(message);
			}
			
			return new CommandResultDto {AffectedId = Guid.Empty, AffectedItems = 1};
		}
    }
}

using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class DenormalizeScheduleCommandHandler : IHandleCommand<DenormalizeScheduleCommandDto>
    {
		private readonly IServiceBusEventPublisher _busSender;

		public DenormalizeScheduleCommandHandler(IServiceBusEventPublisher busSender)
		{
			_busSender = busSender;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(DenormalizeScheduleCommandDto command)
		{
			if (!_busSender.EnsureBus())
			{
				throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the denormalizer.");
			}

			var message = new ScheduleChangedEvent
			              	{
			              		StartDateTime = DateTime.SpecifyKind(command.StartDateTime, DateTimeKind.Utc),
			              		EndDateTime = DateTime.SpecifyKind(command.EndDateTime, DateTimeKind.Utc),
			              		ScenarioId = command.ScenarioId,
			              		PersonId = command.PersonId
			              	};
			
			_busSender.Publish(message);

			command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
    }
}

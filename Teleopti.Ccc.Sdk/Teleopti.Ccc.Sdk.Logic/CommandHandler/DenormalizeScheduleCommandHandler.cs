using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class DenormalizeScheduleCommandHandler : IHandleCommand<DenormalizeScheduleCommandDto>
    {
    	private readonly IServiceBusSender _busSender;

		public DenormalizeScheduleCommandHandler(IServiceBusSender busSender)
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

			var identity = (ITeleoptiIdentity) TeleoptiPrincipal.Current.Identity;
			var message = new ScheduleChangedEvent
			              	{
			              		BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
			              		Datasource = identity.DataSource.Application.Name,
			              		Timestamp = DateTime.UtcNow,
			              		StartDateTime = DateTime.SpecifyKind(command.StartDateTime, DateTimeKind.Utc),
			              		EndDateTime = DateTime.SpecifyKind(command.EndDateTime, DateTimeKind.Utc),
			              		ScenarioId = command.ScenarioId,
			              		PersonId = command.PersonId
			              	};

			_busSender.Send(message);

			command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
    }
}

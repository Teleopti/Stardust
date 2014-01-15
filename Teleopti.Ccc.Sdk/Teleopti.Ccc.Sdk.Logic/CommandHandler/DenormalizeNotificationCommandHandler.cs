using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class DenormalizeNotificationCommandHandler : IHandleCommand<DenormalizeNotificationCommandDto>
	{
		private readonly IServiceBusEventPublisher _busSender;

		public DenormalizeNotificationCommandHandler(IServiceBusEventPublisher busSender)
		{
			_busSender = busSender;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(DenormalizeNotificationCommandDto command)
		{
			if (!_busSender.EnsureBus())
			{
				throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the denormalizer.");
			}

			var message = new ProcessDenormalizeQueue();
			
            _busSender.Publish(message);
			command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
	}
}
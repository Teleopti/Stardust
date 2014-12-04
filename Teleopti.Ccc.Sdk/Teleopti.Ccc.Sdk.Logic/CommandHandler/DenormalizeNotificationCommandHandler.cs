using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class DenormalizeNotificationCommandHandler : IHandleCommand<DenormalizeNotificationCommandDto>
	{
		private readonly IMessagePopulatingServiceBusSender _busSender;

		public DenormalizeNotificationCommandHandler(IMessagePopulatingServiceBusSender busSender)
		{
			_busSender = busSender;
		}

		public void Handle(DenormalizeNotificationCommandDto command)
		{
			var message = new ProcessDenormalizeQueue();
			
            _busSender.Send(message, true);
			command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
	}
}
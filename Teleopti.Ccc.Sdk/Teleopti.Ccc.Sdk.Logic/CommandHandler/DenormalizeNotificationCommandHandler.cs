using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class DenormalizeNotificationCommandHandler : IHandleCommand<DenormalizeNotificationCommandDto>
	{
		private readonly IServiceBusSender _busSender;

		public DenormalizeNotificationCommandHandler(IServiceBusSender busSender)
		{
			_busSender = busSender;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CommandResultDto Handle(DenormalizeNotificationCommandDto command)
		{
			if (!_busSender.EnsureBus())
			{
				throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the denormalizer.");
			}

			var identity = (TeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
			var message = new ProcessDenormalizeQueue
			              	{
			              		BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
			              		Datasource = identity.DataSource.Application.Name,
			              		Timestamp = DateTime.UtcNow
			              	};

			_busSender.NotifyServiceBus(message);
			return new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
	}
}
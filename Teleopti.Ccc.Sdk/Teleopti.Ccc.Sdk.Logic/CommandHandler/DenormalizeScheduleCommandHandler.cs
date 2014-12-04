﻿using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class DenormalizeScheduleCommandHandler : IHandleCommand<DenormalizeScheduleCommandDto>
    {
		private readonly IMessagePopulatingServiceBusSender _busSender;

		public DenormalizeScheduleCommandHandler(IMessagePopulatingServiceBusSender busSender)
		{
			_busSender = busSender;
		}

		public void Handle(DenormalizeScheduleCommandDto command)
		{
			var message = new ScheduleChangedEvent
			              	{
			              		StartDateTime = DateTime.SpecifyKind(command.StartDateTime, DateTimeKind.Utc),
			              		EndDateTime = DateTime.SpecifyKind(command.EndDateTime, DateTimeKind.Utc),
			              		ScenarioId = command.ScenarioId,
			              		PersonId = command.PersonId
			              	};
			
			_busSender.Send(message, true);

			command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
    }
}

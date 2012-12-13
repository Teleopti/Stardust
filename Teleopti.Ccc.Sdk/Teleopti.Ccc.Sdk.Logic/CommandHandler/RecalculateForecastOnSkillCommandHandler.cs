using System;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class RecalculateForecastOnSkillCommandHandler : IHandleCommand<RecalculateForecastOnSkillCommandDto>
	{
		private readonly IServiceBusSender _busSender;

		public RecalculateForecastOnSkillCommandHandler(IServiceBusSender busSender)
		{
			_busSender = busSender;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public CommandResultDto Handle(RecalculateForecastOnSkillCommandDto command)
		{
			
			if (!_busSender.EnsureBus())
			{
				throw new FaultException("The outgoing queue for the service bus is not available. Cannot continue with the forecast.");
			}

			//todo create correct message
			var identity = (ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
			var message = new RecalculateForecastOnSkillMessage
			{
				SkillId = command.SkillId,
				BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault(Guid.Empty),
				Datasource = identity.DataSource.Application.Name,
				Timestamp = DateTime.UtcNow,
				ScenarioId = command.ScenarioId
			};
			_busSender.NotifyServiceBus(message);
			return new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
	}
}
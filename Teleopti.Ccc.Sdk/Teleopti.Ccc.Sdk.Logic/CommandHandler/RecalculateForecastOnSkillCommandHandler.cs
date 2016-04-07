using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class RecalculateForecastOnSkillCommandHandler : IHandleCommand<RecalculateForecastOnSkillCollectionCommandDto>
	{
		private readonly IMessagePopulatingServiceBusSender _busSender;

		public RecalculateForecastOnSkillCommandHandler(IMessagePopulatingServiceBusSender busSender)
		{
			_busSender = busSender;
		}

		public void Handle(RecalculateForecastOnSkillCollectionCommandDto command)
		{
			var principal = TeleoptiPrincipal.CurrentPrincipal;
			var person = ((IUnsafePerson)principal).Person;
			var message = new RecalculateForecastOnSkillCollectionEvent
				{
					SkillCollection = new Collection<RecalculateForecastOnSkill>(),
					ScenarioId = command.ScenarioId,
					OwnerPersonId = person.Id.GetValueOrDefault()
				};
			foreach (var model in command.WorkloadOnSkillSelectionDtos)
			{
				message.SkillCollection.Add(
					new RecalculateForecastOnSkill
						{
							SkillId = model.SkillId,
							WorkloadIds = new Collection<Guid>(model.WorkloadId)
						});

			}

			_busSender.Send(message, true);

			command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
	}
}
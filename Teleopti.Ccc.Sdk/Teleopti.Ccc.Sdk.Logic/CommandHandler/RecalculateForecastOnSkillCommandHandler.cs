using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class RecalculateForecastOnSkillCommandHandler : IHandleCommand<RecalculateForecastOnSkillCollectionCommandDto>
	{
	    private readonly IEventPublisher _publisher;
	    private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly ILoggedOnUser _loggedOnUser;

		public RecalculateForecastOnSkillCommandHandler(IEventPublisher publisher,
            IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator,
			ILoggedOnUser loggedOnUser)
	    {
	        _publisher = publisher;
	        _eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_loggedOnUser = loggedOnUser;
		}

	    public void Handle(RecalculateForecastOnSkillCollectionCommandDto command)
		{
			var principal = TeleoptiPrincipalForLegacy.CurrentPrincipal;
			var person = _loggedOnUser.CurrentUser();
			var @event = new RecalculateForecastOnSkillCollectionEvent
				{
					SkillCollection = new Collection<RecalculateForecastOnSkill>(),
					ScenarioId = command.ScenarioId,
					OwnerPersonId = person.Id.GetValueOrDefault()
				};
			foreach (var model in command.WorkloadOnSkillSelectionDtos)
			{
                @event.SkillCollection.Add(
					new RecalculateForecastOnSkill
						{
							SkillId = model.SkillId,
							WorkloadIds = new Collection<Guid>(model.WorkloadId)
						});
			}

            _eventInfrastructureInfoPopulator.PopulateEventContext(@event);
            _publisher.Publish(@event);

            command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
		}
	}
}
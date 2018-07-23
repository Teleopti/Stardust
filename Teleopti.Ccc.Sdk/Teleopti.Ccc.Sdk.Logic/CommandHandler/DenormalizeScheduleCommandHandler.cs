using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
	public class DenormalizeScheduleCommandHandler : IHandleCommand<DenormalizeScheduleCommandDto>, IInitiatorIdentifier
	{
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IInitiatorIdentifierScope _initiatorIdentifierScope;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;


		public DenormalizeScheduleCommandHandler(IEventPopulatingPublisher eventPopulatingPublisher,
			IPersonRepository personRepository, IScenarioRepository scenarioRepository,
			IInitiatorIdentifierScope initiatorIdentifierScope, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_eventPopulatingPublisher = eventPopulatingPublisher;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_initiatorIdentifierScope = initiatorIdentifierScope;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Handle(DenormalizeScheduleCommandDto command)
		{
			var personExists = false;
			var scenarioExists = false;

			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				personExists = _personRepository.Get(command.PersonId) != null;
				scenarioExists = _scenarioRepository.Get(command.ScenarioId) != null;
			}

			if (!personExists || !scenarioExists ||
				command.StartDateTime == DateTime.MinValue ||
				command.EndDateTime == DateTime.MinValue ||
				command.StartDateTime > command.EndDateTime
			)
			{
				command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 0 };
			}
			else
			{
				var message = new ScheduleChangedEvent
				{
					StartDateTime = DateTime.SpecifyKind(command.StartDateTime, DateTimeKind.Utc),
					EndDateTime = DateTime.SpecifyKind(command.EndDateTime, DateTimeKind.Utc),
					ScenarioId = command.ScenarioId,
					PersonId = command.PersonId
				};
				using (_initiatorIdentifierScope.OnThisThreadUse(this))
				{
					_eventPopulatingPublisher.Publish(message);
				}

				command.Result = new CommandResultDto { AffectedId = Guid.Empty, AffectedItems = 1 };
			}

		}

		public Guid InitiatorId => new Guid("2406c0c3-123b-4a00-902f-ceac53cb835f");
	}
}

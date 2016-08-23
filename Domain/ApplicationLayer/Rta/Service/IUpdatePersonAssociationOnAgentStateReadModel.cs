using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IUpdatePersonAssociationOnAgentStateReadModel
	{
		void Update(PersonAssociationChangedEvent @event);
	}

	public class DontUpdatePersonAssociationOnAgentStateReadModel : IUpdatePersonAssociationOnAgentStateReadModel
	{
		public void Update(PersonAssociationChangedEvent @event)
		{
		}
	}

	public class UpdatePersonAssociationOnAgentStateReadModel : IUpdatePersonAssociationOnAgentStateReadModel
	{
		private readonly IAgentStateReadModelPersister _persister;

		public UpdatePersonAssociationOnAgentStateReadModel(IAgentStateReadModelPersister persister)
		{
			_persister = persister;
		}

		public void Update(PersonAssociationChangedEvent @event)
		{
			var existing = _persister.Get(@event.PersonId);
			if (existing == null)
				return;

			existing.TeamId = @event.TeamId;
			existing.SiteId = @event.SiteId;
			_persister.Persist(existing);
		}
	}
}
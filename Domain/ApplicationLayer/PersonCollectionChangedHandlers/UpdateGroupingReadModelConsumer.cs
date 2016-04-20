using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class UpdateGroupingReadModelConsumer :
		IHandleEvent<PersonCollectionChangedEvent>,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

		public UpdateGroupingReadModelConsumer(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

		public void Handle(PersonCollectionChangedEvent @event)
		{
			_groupingReadOnlyRepository.UpdateGroupingReadModel(@event.PersonIdCollection);
		}
	}
}

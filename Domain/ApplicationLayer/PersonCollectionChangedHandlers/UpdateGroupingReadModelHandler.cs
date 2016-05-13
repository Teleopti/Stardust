using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
#pragma warning disable 618
    [UseNotOnToggle(Toggles.PersonCollectionChanged_ToHangfire_38420)]
    public class UpdateGroupingReadModelConsumer :
		IHandleEvent<PersonCollectionChangedEvent>,
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

    [UseOnToggle(Toggles.PersonCollectionChanged_ToHangfire_38420)]
    public class UpdateGroupingReadModelHandler :
        IHandleEvent<PersonCollectionChangedEvent>,
        IRunOnHangfire
    {
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

        public UpdateGroupingReadModelHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository)
        {
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
        }

        [AsSystem, UnitOfWork]
        public virtual void Handle(PersonCollectionChangedEvent @event)
        {
            _groupingReadOnlyRepository.UpdateGroupingReadModel(@event.PersonIdCollection);
        }
    }
}

using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
#pragma warning disable 618
    [UseNotOnToggle(Toggles.PersonCollectionChanged_ToHangfire_38420)]
	public class PersonFinderReadOnlyUpdaterBus : 
		IHandleEvent<PersonCollectionChangedEvent>,
		IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

        public PersonFinderReadOnlyUpdaterBus(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
        {
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
        }

	    public void Handle(PersonCollectionChangedEvent @event)
	    {
			_personFinderReadOnlyRepository.UpdateFindPerson(@event.PersonIdCollection);
		}
	}

    [UseOnToggle(Toggles.PersonCollectionChanged_ToHangfire_38420)]
    public class PersonFinderReadOnlyUpdater :
        IHandleEvent<PersonCollectionChangedEvent>,
        IRunOnHangfire
    {
        private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

        public PersonFinderReadOnlyUpdater(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
        {
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
        }

        [AsSystem, UnitOfWork]
        public virtual void Handle(PersonCollectionChangedEvent @event)
        {
            _personFinderReadOnlyRepository.UpdateFindPerson(@event.PersonIdCollection);
        }
    }
}

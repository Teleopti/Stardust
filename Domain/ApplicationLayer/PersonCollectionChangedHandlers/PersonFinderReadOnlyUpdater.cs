using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
    public class PersonFinderReadOnlyUpdater :
        IHandleEvent<PersonCollectionChangedEvent>,
        IRunOnHangfire
    {
        private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

        public PersonFinderReadOnlyUpdater(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
        {
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
        }

        [ImpersonateSystem, UnitOfWork]
        public virtual void Handle(PersonCollectionChangedEvent @event)
        {
            _personFinderReadOnlyRepository.UpdateFindPerson(@event.PersonIdCollection.ToArray());
        }
    }
}

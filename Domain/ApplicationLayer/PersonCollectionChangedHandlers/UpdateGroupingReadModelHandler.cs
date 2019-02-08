using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
    public class UpdateGroupingReadModelHandler :
        IHandleEvent<PersonCollectionChangedEvent>,
        IRunOnHangfire
    {
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

        public UpdateGroupingReadModelHandler(IGroupingReadOnlyRepository groupingReadOnlyRepository)
        {
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
        }

        [ImpersonateSystem, UnitOfWork]
        public virtual void Handle(PersonCollectionChangedEvent @event)
        {
            _groupingReadOnlyRepository.UpdateGroupingReadModel(@event.PersonIdCollection.ToArray());
        }
    }
}

using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class UpdateFindPersonDataConsumer : ConsumerOf<PersonPeriodChangedMessage>
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;
	    private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

	    public UpdateFindPersonDataConsumer(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
        {
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
            _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
        }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(PersonPeriodChangedMessage message)
		{
	        using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                _personFinderReadOnlyRepository.UpdateFindPersonData(message.PersonIdCollection);
                uow.PersistAll();
            }
		}
	}
}

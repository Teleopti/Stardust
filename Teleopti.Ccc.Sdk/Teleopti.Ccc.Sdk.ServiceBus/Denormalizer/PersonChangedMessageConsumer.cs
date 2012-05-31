using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class PersonChangedMessageConsumer : ConsumerOf<PersonChangedMessage>
	{
        //private readonly IUpdatePersonFinderReadModel  _updatePersonFinderReadModel;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

        public PersonChangedMessageConsumer(IUnitOfWorkFactory unitOfWorkFactory, IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
		{
            _unitOfWorkFactory = unitOfWorkFactory;
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(PersonChangedMessage  message)
		{
                _personFinderReadOnlyRepository.UpdateFindPerson(message.Ids);
		}
	}
}

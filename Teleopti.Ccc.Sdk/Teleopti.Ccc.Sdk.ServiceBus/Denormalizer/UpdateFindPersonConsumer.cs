using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class UpdateFindPersonConsumer : ConsumerOf<PersonChangedMessage>
	{
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

        public UpdateFindPersonConsumer(IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
		{
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(PersonChangedMessage  message)
		{
            _personFinderReadOnlyRepository.UpdateFindPerson(message.PersonIdCollection);
		}
	}
}

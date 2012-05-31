
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class UpdateFindPersonDataConsumer : ConsumerOf<PersonChangedMessage>
	{
        //private readonly IUpdatePersonFinderReadModel  _updatePersonFinderReadModel;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IPersonFinderReadOnlyRepository _personFinderReadOnlyRepository;

        public UpdateFindPersonDataConsumer(IUnitOfWorkFactory unitOfWorkFactory, IPersonFinderReadOnlyRepository personFinderReadOnlyRepository)
		{
            _unitOfWorkFactory = unitOfWorkFactory;
            _personFinderReadOnlyRepository = personFinderReadOnlyRepository;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(PersonChangedMessage  message)
		{
            //_updatePersonFinderReadModel.Execute(message.IsPerson , message.Ids);
            _personFinderReadOnlyRepository.UpdateFindPersonData( message.Ids);
		}
	}
}


using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class UpdateGroupingReadModelConsumer : ConsumerOf<PersonChangedMessage >
	{
        //private readonly IUpdatePersonFinderReadModel  _updatePersonFinderReadModel;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

        public UpdateGroupingReadModelConsumer(IUnitOfWorkFactory unitOfWorkFactory, IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
            _unitOfWorkFactory = unitOfWorkFactory;
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(PersonChangedMessage message)
		{
            //_updatePersonFinderReadModel.Execute(message.IsPerson , message.Ids);
            _groupingReadOnlyRepository.UpdateGroupingReadModel(message.Ids );
		}
	}
}

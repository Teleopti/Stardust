

using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class UpdateGroupingReadModelDataConsumer : ConsumerOf<PersonPeriodChangedMessage   >
	{
       private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

        public UpdateGroupingReadModelDataConsumer(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
            
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(PersonPeriodChangedMessage message)
		{
             _groupingReadOnlyRepository.UpdateGroupingReadModelData(  message.Ids);
		}
	}
}


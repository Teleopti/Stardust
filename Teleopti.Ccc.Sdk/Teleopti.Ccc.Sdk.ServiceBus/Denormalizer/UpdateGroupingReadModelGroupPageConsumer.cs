
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class UpdateGroupingReadModelGroupPageConsumer : ConsumerOf<GroupPageChangedMessage  >
	{
         private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

        public UpdateGroupingReadModelGroupPageConsumer( IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
           
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(GroupPageChangedMessage message)
		{
              _groupingReadOnlyRepository.UpdateGroupingReadModelGroupPage( message.Ids);
		}
	}
}

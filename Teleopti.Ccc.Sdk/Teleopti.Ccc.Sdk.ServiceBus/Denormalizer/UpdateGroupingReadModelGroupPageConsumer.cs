
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class UpdateGroupingReadModelGroupPageConsumer : ConsumerOf<GroupPageChangedMessage  >
	{
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;

        public UpdateGroupingReadModelGroupPageConsumer(IGroupingReadOnlyRepository groupingReadOnlyRepository)
		{
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(GroupPageChangedMessage message)
		{
            _groupingReadOnlyRepository.UpdateGroupingReadModelGroupPage(message.GroupPageIdCollection);
		}
	}
}

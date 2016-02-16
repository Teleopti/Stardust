
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
	    private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

	    public UpdateGroupingReadModelGroupPageConsumer(IGroupingReadOnlyRepository groupingReadOnlyRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_groupingReadOnlyRepository = groupingReadOnlyRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(GroupPageChangedMessage message)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_groupingReadOnlyRepository.UpdateGroupingReadModelGroupPage(message.GroupPageIdCollection);
				uow.PersistAll();
			}
		}
	}
}

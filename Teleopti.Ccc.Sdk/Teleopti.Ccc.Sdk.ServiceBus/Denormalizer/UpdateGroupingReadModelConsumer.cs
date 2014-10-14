﻿
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
    public class UpdateGroupingReadModelConsumer : ConsumerOf<PersonChangedMessage >
	{
        private readonly IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public UpdateGroupingReadModelConsumer(IGroupingReadOnlyRepository groupingReadOnlyRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
            _groupingReadOnlyRepository = groupingReadOnlyRepository;
		    _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(PersonChangedMessage message)
		{
			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				_groupingReadOnlyRepository.UpdateGroupingReadModel(message.PersonIdCollection);
				uow.PersistAll();
			}
		}
	}
}

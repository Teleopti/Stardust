﻿

using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
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
            _groupingReadOnlyRepository.UpdateGroupingReadModelData(  message.PersonIdCollection);
		}
	}
}


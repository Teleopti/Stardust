using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventSubscriberInserter : ObjectInserter<IEventSubscriber>
    {

        public EventSubscriberInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IEventSubscriber> GetMapper()
        {
            MapperBase<IEventSubscriber> mapper = new EventSubscriberMapper(ConnectionString);
            return mapper;
        }

    }
}
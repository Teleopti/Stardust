using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventMessageInserter : ObjectInserter<IEventMessage>
    {

        public EventMessageInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IEventMessage> GetMapper()
        {
            IMapperBase<IEventMessage> mapper = new EventMessageMapper(ConnectionString);
            return mapper;
        }
    }
}
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventFilterInserter : ObjectInserter<IEventFilter>
    {
        public EventFilterInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IEventFilter> GetMapper()
        {
            IMapperBase<IEventFilter> mapper = new EventFilterMapper(ConnectionString);
            return mapper; 
        }
    }
}
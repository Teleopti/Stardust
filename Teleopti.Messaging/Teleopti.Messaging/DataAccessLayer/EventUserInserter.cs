using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class EventUserInserter : ObjectInserter<IEventUser>
    {
        public EventUserInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IEventUser> GetMapper()
        {
            MapperBase<IEventUser> mapper = new EventUserMapper(ConnectionString);
            return mapper;
        }
    }
}
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class HeartbeatInserter : ObjectInserter<IEventHeartbeat>
    {

        public HeartbeatInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IEventHeartbeat> GetMapper()
        {
            IMapperBase<IEventHeartbeat> mapper = new HeartbeatMapper(ConnectionString);
            return mapper;
        }

    }
}

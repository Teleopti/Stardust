using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ReceiptInserter : ObjectInserter<IEventReceipt>
    {
        public ReceiptInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IEventReceipt> GetMapper()
        {
            IMapperBase<IEventReceipt> mapper = new ReceiptMapper(ConnectionString);
            return mapper;
        }

    }
}

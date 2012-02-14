using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class AddressInserter : ObjectInserter<IAddressInformation>
    {

        public AddressInserter(string connectionString) : base(connectionString)
        {
        }

        protected override IMapperBase<IAddressInformation> GetMapper()
        {
            IMapperBase<IAddressInformation> mapper = new AddressMapper(ConnectionString);
            return mapper;
        }
    }
}

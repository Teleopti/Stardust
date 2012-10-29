using System;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class AddressDeleter : ObjectDeleter
    {
		private static ILog Logger = LogManager.GetLogger(typeof(AddressDeleter));

        public AddressDeleter(string connectionString) : base(connectionString)
        {
        }

        public void DeleteAddresses(IAddressInformation addressInfo)
        {
            try
            {
                DeleteAddedRecord("msg.sp_Address_Delete", "@MessageBrokerId", addressInfo.AddressId);
            }
            catch (Exception exc)
            {
                Logger.Error("DeleteAddresses(IMessageInfo multicastAddressInfo) failed.", exc);
                throw new DatabaseException("AddressDeleter(IAddressInfo addressInfo)", exc);
            }
        }
    }
}

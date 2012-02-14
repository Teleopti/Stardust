using System;
using System.Diagnostics;
using System.Globalization;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class AddressDeleter : ObjectDeleter
    {
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
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "DeleteAddresses(IMessageInfo multicastAddressInfo) failed. {0}.", exc));
                throw new DatabaseException("AddressDeleter(IAddressInfo addressInfo)", exc);
            }
        }

    }
}

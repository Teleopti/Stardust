using System;
using System.Diagnostics;
using System.Globalization;
using Teleopti.Core;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Logging;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ConfigurationDeleter : ObjectDeleter
    {
        public ConfigurationDeleter(string connectionString) : base(connectionString)
        {
        }

        public void DeleteConfiguration(IConfigurationInfo configurationInfo)
        {
            try
            {
                DeleteAddedRecord("msg.sp_Configuration_Delete", "@ConfigurationId", configurationInfo.ConfigurationId);
            }
            catch (Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), String.Format(CultureInfo.InvariantCulture, "DeleteAddresses(IMessageInfo multicastAddressInfo) failed. {0}.", exc));
                throw new DatabaseException("DeleteConfiguration(IConfigurationInfo configurationInfo)", exc);
            }
        }

    }
}

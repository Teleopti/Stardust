using System;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Core;
using Teleopti.Messaging.Exceptions;
using log4net;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class ConfigurationDeleter : ObjectDeleter
    {
		private static ILog Logger = LogManager.GetLogger(typeof(ConfigurationDeleter));

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
                Logger.Error("DeleteAddresses(IMessageInfo multicastAddressInfo) failed.", exc);
                throw new DatabaseException("DeleteConfiguration(IConfigurationInfo configurationInfo)", exc);
            }
        }

    }
}

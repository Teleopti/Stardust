using System;
using System.Globalization;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.DataAccessLayer;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    internal static class MessageBrokerFactory
    {
        internal static MessageBrokerDto RetrieveMessageBrokerConfigurations()
        {
            string connectionString = TeleoptiCccSdkService.PublishedSettings["MessageBroker"];
        	
			Uri serverUrl;
        	if (Uri.TryCreate(connectionString,UriKind.Absolute,out serverUrl))
        	{
				return new MessageBrokerDto { ConnectionString = connectionString };
        	}

        	ConfigurationInfoReader reader = new ConfigurationInfoReader(connectionString);

            MessageBrokerDto messageBroker = new MessageBrokerDto{ConnectionString = connectionString};
            foreach (IConfigurationInfo configInfo in reader.Execute())
            {
                if(configInfo.ConfigurationName=="Port")
                {
                    messageBroker.Port = Convert.ToInt32(configInfo.ConfigurationValue,CultureInfo.InvariantCulture);
                }
                else if (configInfo.ConfigurationName == "Server")
                {
                    messageBroker.Server = configInfo.ConfigurationValue;
                }
                else if (configInfo.ConfigurationName == "Threads")
                {
                    messageBroker.Threads = Convert.ToInt32(configInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                }
                else if (configInfo.ConfigurationName == "Intervall")
                {
                    messageBroker.Interval = Convert.ToDouble(configInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                }
                else if (configInfo.ConfigurationName == "GeneralThreadPoolThreads")
                {
                    messageBroker.GeneralThreadPoolThreads = Convert.ToInt32(configInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                }
                else if (configInfo.ConfigurationName == "DatabaseThreadPoolThreads")
                {
                    messageBroker.DatabaseThreadPoolThreads = Convert.ToInt32(configInfo.ConfigurationValue, CultureInfo.InvariantCulture);
                }
                else if (configInfo.ConfigurationName == "ReceiptThreadPoolThreads")
                {
                    messageBroker.ReceiptThreadPoolThreads = Convert.ToInt32(configInfo.ConfigurationValue, CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            return messageBroker;
        }
    }
}

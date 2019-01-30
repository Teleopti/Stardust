using System;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    internal static class MessageBrokerFactory
    {
        internal static MessageBrokerDto RetrieveMessageBrokerConfigurations()
        {
					string connectionString = StateHolder.Instance.StateReader.ApplicationScopeData_DONTUSE.AppSettings["MessageBroker"];
        	
			Uri serverUrl;
        	if (Uri.TryCreate(connectionString,UriKind.Absolute,out serverUrl))
        	{
				return new MessageBrokerDto { ConnectionString = connectionString };
        	}
			return new MessageBrokerDto();
        }
    }
}

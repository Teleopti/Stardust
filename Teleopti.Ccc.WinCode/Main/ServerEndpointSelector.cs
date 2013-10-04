﻿using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Configuration;

namespace Teleopti.Ccc.WinCode.Main
{
    public interface IServerEndpointSelector
    {
        List<string> GetEndpointNames();
    }
    public class ServerEndpointSelector : IServerEndpointSelector
	{
		public List<string> GetEndpointNames()
		{
			var clientSettings = ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection;
			if (clientSettings != null)
			{
				return clientSettings.Endpoints
				                     .Cast<ChannelEndpointElement>()
				                     .Where(
					                     e =>
					                     e.Contract ==
					                     "Teleopti.Ccc.Sdk.Common.Contracts.ITeleoptiCccSdkInternal")
				                     .Select(e => e.Name)
				                     .ToList();
			}
			return new List<string>();
		}
	}
}

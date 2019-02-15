using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Stardust.Node;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class FetchNodeConfiguration
	{
		private readonly IInstallationEnvironment _installationEnvironment;

		public FetchNodeConfiguration(IInstallationEnvironment installationEnvironment)
		{
			_installationEnvironment = installationEnvironment;
		}

		public NodeConfiguration GetNodeConfiguration(int port, string nodeName, string fixedNodeIp,
			Uri managerLocation, Assembly handlerAssembly, int pingToManagerSeconds, int sendDetailsToManagerMilliSeconds,
			bool enableGC)
		{
			if (_installationEnvironment.IsAzure)
			{
				var ipAddress = getIPAddress();
				if (!string.IsNullOrEmpty(fixedNodeIp))
				{
					ipAddress = fixedNodeIp;
				}
				return new NodeConfiguration(
					managerLocation,
					handlerAssembly,
					port,
					nodeName,
					pingToManagerSeconds,
					sendDetailsToManagerMilliSeconds,
					IPAddress.Parse(ipAddress), enableGC
				);
			}
			return new NodeConfiguration(
				managerLocation,
				handlerAssembly,
				port,
				nodeName,
				pingToManagerSeconds,
				sendDetailsToManagerMilliSeconds, enableGC
			);

		}

		private string getIPAddress()
		{
			var localIp = "?";
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIp = ip.ToString();
				}
			}
			return localIp;
		}
	}
}
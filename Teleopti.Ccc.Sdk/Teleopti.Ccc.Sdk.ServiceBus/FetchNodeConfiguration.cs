using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Stardust.Node;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	public class FetchNodeConfigurationToggleOff
	{
		public NodeConfiguration GetNodeConfiguration(int port, string nodeName, string fixedNodeIp,
			Uri managerLocation, Assembly handlerAssembly, int pingToManagerSeconds, int sendDetailsToManagerMilliSeconds,
			bool enableGC)
		{
			if (string.IsNullOrEmpty(fixedNodeIp))
			{
				return new NodeConfiguration(
					managerLocation,
					handlerAssembly,
					port,
					nodeName,
					pingToManagerSeconds,
					sendDetailsToManagerMilliSeconds, enableGC
				);
			}

			return new NodeConfiguration(
				managerLocation,
				handlerAssembly,
				port,
				nodeName,
				pingToManagerSeconds,
				sendDetailsToManagerMilliSeconds,
				IPAddress.Parse(fixedNodeIp), enableGC
			);

		}
	}

	public class FetchNodeConfigurationToggleOn
	{
		public NodeConfiguration GetNodeConfiguration(int port, string nodeName, string fixedNodeIp,
			Uri managerLocation, Assembly handlerAssembly, int pingToManagerSeconds, int sendDetailsToManagerMilliSeconds,
			bool enableGC)
		{
			if (isAzure())
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

		private bool isAzure()
		{
			var tennConn = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			return tennConn.DataSource.Contains("database.windows.net");
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
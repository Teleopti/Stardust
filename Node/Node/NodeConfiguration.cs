using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Stardust.Node
{
	public class NodeConfiguration
	{
		public NodeConfiguration(Uri managerLocation, Assembly handlerAssembly, int port, string nodeName, int pingToManagerSeconds, int sendDetailsToManagerMilliSeconds)
		{
			BaseAddress = CreateNodeAddress(port);
			ManagerLocation = managerLocation;
			HandlerAssembly = handlerAssembly;
			NodeName = nodeName;
			PingToManagerSeconds = pingToManagerSeconds;
			SendDetailsToManagerMilliSeconds = sendDetailsToManagerMilliSeconds;

			ValidateParameters();
		}
		
		public Uri BaseAddress { get; private set; }
		public Uri ManagerLocation { get; }
		public string NodeName { get; private set; }
		public Assembly HandlerAssembly { get; }
		public double PingToManagerSeconds { get; }
		public double SendDetailsToManagerMilliSeconds { get; private set; }

		private void ValidateParameters()
		{

			if (ManagerLocation == null)
			{
				throw new ArgumentNullException(nameof(ManagerLocation));
			}
			if (HandlerAssembly == null)
			{
				throw new ArgumentNullException(nameof(HandlerAssembly));
			}
			if (PingToManagerSeconds <= 0)
			{
				throw new ArgumentNullException(nameof(PingToManagerSeconds));
			}
		}

		private static string GetIpAddress()
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
		
		private Uri CreateNodeAddress(int port)
		{
			if (port <= 0)
			{
				throw new ArgumentNullException(nameof(port));
			}
			return new Uri("http://" + GetIpAddress() + ":" + port + "/");
		}
	}
}
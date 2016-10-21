using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Stardust.Node
{
	public class NodeConfiguration
	{
		public void SetUp(Uri managerLocation, Assembly handlerAssembly, int port, string nodeName, int pingToManagerSeconds)
		{
			BaseAddress = CreateNodeAddress(port);
			ManagerLocation = managerLocation;
			HandlerAssembly = handlerAssembly;
			NodeName = nodeName;
			PingToManagerSeconds = pingToManagerSeconds;

			ValidateParameters();
		}
		
		public Uri BaseAddress { get; private set; }
		public Uri ManagerLocation { get; private set; }
		public string NodeName { get; private set; }
		public Assembly HandlerAssembly { get; private set; }
		public double PingToManagerSeconds { get; private set; }

		private void ValidateParameters()
		{

			if (ManagerLocation == null)
			{
				throw new ArgumentNullException("managerLocation");
			}
			if (HandlerAssembly == null)
			{
				throw new ArgumentNullException("handlerAssembly");
			}
			if (PingToManagerSeconds <= 0)
			{
				throw new ArgumentNullException("pingToManagerSeconds");
			}
		}

		private string GetIPAddress()
		{
			string localIp = "?";
			IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
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
				throw new ArgumentNullException("port");
			}
			return new Uri("http://" + GetIPAddress() + ":" + port + "/");
		}

	}
}
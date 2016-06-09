using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Stardust.Node
{
	public class NodeConfiguration
	{
		public NodeConfiguration()
		{
			BaseAddress = new Uri(CreateBaseAddress());
			ManagerLocation = new Uri(ConfigurationManager.AppSettings["ManagerLocation"]);
			HandlerAssembly = Assembly.Load(ConfigurationManager.AppSettings["HandlerAssembly"]);
			NodeName = ConfigurationManager.AppSettings["NodeName"];
			PingToManagerSeconds = int.Parse(ConfigurationManager.AppSettings["PingToManagerSeconds"]);
			
			ValidateParameters();
		}

		public double PingToManagerSeconds { get; private set; }
		public Uri BaseAddress { get; private set; }
		public Uri ManagerLocation { get; private set; }
		public string NodeName { get; private set; }
		public Assembly HandlerAssembly { get; private set; }

		private void ValidateParameters()
		{
			if (BaseAddress == null)
			{
				throw new ArgumentNullException("baseAddress");
			}
			if (ManagerLocation == null)
			{
				throw new ArgumentNullException("managerLocation");
			}
			if (HandlerAssembly == null)
			{
				throw new ArgumentNullException("handlerAssembly");
			}
			if (string.IsNullOrEmpty(NodeName))
			{
				throw new ArgumentNullException("nodeName");
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


		private string CreateBaseAddress()
		{
			string baseAddress = ConfigurationManager.AppSettings["BaseAddress"];
			if (string.IsNullOrEmpty(baseAddress))
			{
				baseAddress = "http://" + GetIPAddress() + ":" + ConfigurationManager.AppSettings["Port"] + "/";
			}
			return baseAddress;
		}
	}
}
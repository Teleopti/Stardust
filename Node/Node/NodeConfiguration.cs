﻿using System;
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
		
		public Uri BaseAddress { get; }
		public Uri ManagerLocation { get; }
		public string NodeName { get; }
		public Assembly HandlerAssembly { get; }
		public double PingToManagerSeconds { get; }
		public double SendDetailsToManagerMilliSeconds { get; }

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

		private static string GetHostName()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			return host.HostName;
		}

		private Uri CreateNodeAddress(int port)
		{
			if (port <= 0)
			{
				throw new ArgumentNullException(nameof(port));
			}
			return new Uri("http://" + GetHostName() + ":" + port + "/");
		}
	}
}
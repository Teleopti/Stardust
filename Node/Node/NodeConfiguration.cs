﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Stardust.Node
{
	public class NodeConfiguration
	{
		public NodeConfiguration(Uri managerLocation, Assembly handlerAssembly, int port, string nodeName, int pingToManagerSeconds, int sendDetailsToManagerMilliSeconds, bool enableGc)
		{
			BaseAddress = CreateNodeAddress(port);
			ManagerLocation = managerLocation;
			HandlerAssembly = handlerAssembly;
			NodeName = nodeName;
			PingToManagerSeconds = pingToManagerSeconds;
			SendDetailsToManagerMilliSeconds = sendDetailsToManagerMilliSeconds;
			EnableGarbageCollection = enableGc;

			ValidateParameters();
		}

	    public NodeConfiguration(Uri managerLocation, Assembly handlerAssembly, int port, string nodeName,
	        int pingToManagerSeconds, int sendDetailsToManagerMilliSeconds, IPAddress fixedNodeIp, bool enableGc) : this(managerLocation,
	        handlerAssembly, port, nodeName, pingToManagerSeconds, sendDetailsToManagerMilliSeconds, enableGc)
	    {
	        if (fixedNodeIp == null)
	        {
	            throw new ArgumentNullException(nameof(fixedNodeIp));
	        }

            BaseAddress = CreateNodeAddress(port, fixedNodeIp);
        }

        public Uri BaseAddress { get; }
		public Uri ManagerLocation { get; }
		public string NodeName { get; }
		public Assembly HandlerAssembly { get; }
		public double PingToManagerSeconds { get; }
		public double SendDetailsToManagerMilliSeconds { get; }
		public bool EnableGarbageCollection { get; set; }

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

		private Uri CreateNodeAddress(int port, IPAddress fixedIp = null)
		{
			if (port <= 0)
			{
				throw new ArgumentNullException(nameof(port));
			}

		    var nodeUrl = "http://";

            if(fixedIp != null)
            {
                nodeUrl += fixedIp.AddressFamily == AddressFamily.InterNetworkV6 ? "[" + fixedIp + "]" : fixedIp.ToString();
            }
            else
            {
                nodeUrl += GetHostName();
            }

			return new Uri(nodeUrl + ":" + port + "/");
		}
	}
}
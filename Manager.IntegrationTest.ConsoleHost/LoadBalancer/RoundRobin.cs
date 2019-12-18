using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using log4net;
using Manager.IntegrationTest.ConsoleHost.Log4Net;

namespace Manager.IntegrationTest.ConsoleHost.LoadBalancer
{
	public static class RoundRobin
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (RoundRobin));

		private static List<Uri> _hosts;

		private static int _currentIndex;

		public static void AddHost(Uri hostUri)
		{
			_hosts.Add(hostUri);
		}

		public static void DeleteHost(Uri hostUri)
		{
			_hosts.Remove(hostUri);
		}

		public static void Set(List<Uri> hosts)
		{
			Logger.DebugWithLineNumber("Start.");

			_hosts = hosts;

			if (hosts.Any())
			{
				foreach (var host in hosts)
				{
					Logger.DebugWithLineNumber("Load balancer will register manager url : ( " + host + " )");
				}
			}

			Logger.DebugWithLineNumber("Finished.");
		}

		public static Uri Next(HttpRequestMessage request)
		{
			Logger.DebugWithLineNumber("Start.");

			Interlocked.Increment(ref _currentIndex);

			var host = _hosts[_currentIndex % _hosts.Count];

			Logger.DebugWithLineNumber("Load balancer will use manager url : ( " + host + " ) for request : ( " + request.RequestUri + " )");

			Logger.DebugWithLineNumber("Finsihed.");

			return host;
		}
	}
}
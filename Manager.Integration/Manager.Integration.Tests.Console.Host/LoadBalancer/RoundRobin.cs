using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using log4net;
using Manager.IntegrationTest.Console.Host.Helpers;

namespace Manager.IntegrationTest.Console.Host.LoadBalancer
{
	public static class RoundRobin
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof(RoundRobin));

		private static List<Uri> _hosts;

		private static int _currentIndex;

		public static void Set(List<Uri> hosts)
		{
			LogHelper.LogDebugWithLineNumber(Logger,"Start.");

			_hosts = hosts;

			if (hosts.Any())
			{
				foreach (var host in hosts)
				{
					LogHelper.LogDebugWithLineNumber(Logger,
												     "Load balancer will register manager url : ( " + host  + " )");
				}
			}

			LogHelper.LogDebugWithLineNumber(Logger, "Finished.");
		}

		public static Uri Next(HttpRequestMessage request)
		{
			LogHelper.LogDebugWithLineNumber(Logger, "Start.");

			Interlocked.Increment(ref _currentIndex);

			var host = _hosts[_currentIndex % _hosts.Count];

			LogHelper.LogDebugWithLineNumber(Logger,
											"Load balancer will use manager url : ( " + host + " ) for request : ( " + request.RequestUri + " )" );

			LogHelper.LogDebugWithLineNumber(Logger, "Finsihed.");

			return host;
		}
	}
}
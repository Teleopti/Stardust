using System;
using System.Collections.Generic;
using System.Threading;

namespace Manager.IntegrationTest.Console.Host.LoadBalancer
{
	public static class RoundRobin
	{
		private static List<Uri> _hosts;

		private static int _currentIndex;

		public static void Set(List<Uri> hosts)
		{
			_hosts = hosts;
		}

		public static Uri Next()
		{
			Interlocked.Increment(ref _currentIndex);

			var host = _hosts[_currentIndex%_hosts.Count];

			return host;
		}
	}
}
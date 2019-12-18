using System;

namespace Manager.IntegrationTest.ConsoleHost.Models
{
	public class Configuration
	{
		public Configuration(string baseAddress) : this(new Uri(baseAddress))
		{
		}

		public Configuration(Uri baseAddress)
		{
			BaseAddress = baseAddress;
		}

		public Uri BaseAddress { get; private set; }
	}
}
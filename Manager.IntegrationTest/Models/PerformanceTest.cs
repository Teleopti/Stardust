using System;

namespace Manager.IntegrationTest.Models
{
	public class PerformanceTest
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public DateTime Started { get; set; }

		public DateTime Ended { get; set; }

		public double ElapsedInSeconds { get; set; }

		public double ElapsedInMinutes { get; set; }
	}
}
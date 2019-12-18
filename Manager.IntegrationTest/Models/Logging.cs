using System;

namespace Manager.IntegrationTest.Models
{
	public class Logging
	{
		public int Id { get; set; }

		public DateTime Date { get; set; }

		public string Thread { get; set; }

		public string Level { get; set; }

		public string Logger { get; set; }

		public string Message { get; set; }

		public string Exception { get; set; }
	}
}
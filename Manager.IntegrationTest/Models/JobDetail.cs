using System;

namespace Manager.IntegrationTest.Models
{
	public class JobDetail
	{
		public int Id { get; set; }

		public Guid JobId { get; set; }

		public string Detail { get; set; }

		public DateTime Created { get; set; }
	}
}
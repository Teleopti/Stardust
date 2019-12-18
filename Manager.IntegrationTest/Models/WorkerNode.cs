using System;

namespace Manager.IntegrationTest.Models
{
	public class WorkerNode
	{
		public WorkerNode()
		{
			Id = Guid.NewGuid();

			Heartbeat = DateTime.Now;

			Alive = true;
		}

		public Uri Url { get; set; }

		public Guid Id { get; set; }

		public bool Alive { get; set; }

		public DateTime Heartbeat { get; set; }
	}
}
using System;

namespace Stardust.Manager.Models
{
	public class WorkerNode
	{
		public WorkerNode()
		{
			Id = Guid.NewGuid();
			Heartbeat = DateTime.UtcNow;
			Alive = true;
		}

		public Uri Url { get; set; }

		public Guid Id { get; set; }

		public bool Alive { get; set; }

		public DateTime Heartbeat { get; set; }
	}
}
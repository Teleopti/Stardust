using System;

namespace Stardust.Manager.Models
{
	public class WorkerNode
	{
		public Uri Url { get; set; }
		public Guid Id { get; set; }

		public string Alive { get; set; }

		public DateTime Heartbeat { get; set; }
	}
}
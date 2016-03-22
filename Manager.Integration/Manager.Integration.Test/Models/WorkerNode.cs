using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manager.Integration.Test.Models
{
	class WorkerNode
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

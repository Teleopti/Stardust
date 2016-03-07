using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Wfm.Administration.Models.Stardust
{
	public class WorkerNode
	{
		public WorkerNode()
		{
			Id = Guid.NewGuid();

			Heartbeat = DateTime.Now;

			Alive = "true";
		}

		public Uri Url { get; set; }

		public Guid Id { get; set; }

		public string Alive { get; set; }

		public DateTime Heartbeat { get; set; }

		public bool Running { get; set; }
	}
}
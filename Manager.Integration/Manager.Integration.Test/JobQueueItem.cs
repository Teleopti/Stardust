using System;

namespace Manager.Integration.Test
{
	public class JobQueueItem
	{
		public string Name { get; set; }

		public string Serialized { get; set; }

		public string Type { get; set; }

		public string CreatedBy { get; set; }
	}
}
﻿using System;

namespace Manager.Integration.Test.Models
{
	public class JobQueueItem
	{
		public Guid JobId { get; set; }

		public string Name { get; set; }

		public string Serialized { get; set; }

		public string Type { get; set; }

		public string CreatedBy { get; set; }

		public DateTime Created { get; set; }
	}
}
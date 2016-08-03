﻿using System;

namespace Teleopti.Wfm.Administration.Models.Stardust
{
	public class Job
	{
		public Guid JobId { get; set; }

		public string Name { get; set; }

		public string CreatedBy { get; set; }

		public DateTime Created { get; set; }

		public DateTime? Started { get; set; }

		public DateTime? Ended { get; set; }

		public string SentToWorkerNodeUri { get; set; }

		public string Result { get; set; }

		public string Type { get; set; }

		public string Serialized { get; set; }

		public string TotalDuration { get; set; }
	}
}
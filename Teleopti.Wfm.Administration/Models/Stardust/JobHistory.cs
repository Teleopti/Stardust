using System;

namespace Teleopti.Wfm.Administration.Models.Stardust
{
	public class JobHistory
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public string CreatedBy { get; set; }

		public DateTime Created { get; set; }

		public DateTime? Started { get; set; }

		public DateTime? Ended { get; set; }

		public string SentTo { get; set; }

		public string Result { get; set; }
	}
}
using System;

namespace Stardust.Manager.Models
{
	public class JobDetail
	{
		public DateTime Created { get; set; }

		public string Detail { get; set; }

		public Guid JobId { get; set; }
		public int Id { get; set; }

	}
}
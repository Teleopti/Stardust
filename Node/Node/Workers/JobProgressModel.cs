using System;

namespace Stardust.Node.Workers
{
	public class JobProgressModel
	{
		public Guid JobId { get; set; }
		public string ProgressDetail { get; set; }
	}
}
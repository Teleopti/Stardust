using System;

namespace Stardust.Node.Entities
{
	public class JobDetailEntity
	{
		public Guid JobId { get; set; }
		public string Detail { get; set; }
		public DateTime? Created { get; set; }
		public bool Sent { get; set; }
	}
}
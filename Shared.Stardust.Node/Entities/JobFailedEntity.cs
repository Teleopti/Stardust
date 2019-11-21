using System;

namespace Stardust.Node.Entities
{
	public class JobFailedEntity 
	{
		public Guid JobId { get; set; }

		public AggregateException AggregateException { get; set; }

		public DateTime? Created { get; set; }
	}
}
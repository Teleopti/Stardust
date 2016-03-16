using System;

namespace Stardust.Node.Entities
{
	public class JobFailedModel
	{
		public Guid JobId { get; set; }

		public AggregateException AggregateException { get; set; }
	}
}
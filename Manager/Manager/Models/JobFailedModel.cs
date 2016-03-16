using System;

namespace Stardust.Manager.Models
{
	public class JobFailedModel
	{
		public Guid JobId { get; set; }

		public AggregateException AggregateException { get; set; }
	}
}
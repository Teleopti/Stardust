using System;

namespace Stardust.Node.Interfaces
{
	public interface IJobQueueItem
	{
		Guid JobId { get; set; }

		string Name { get; set; }

		string Serialized { get; set; }

		string Type { get; set; }

		string CreatedBy { get; set; }

		DateTime? Created { get; set; }
	}
}
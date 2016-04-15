using System;

namespace Stardust.Node.Interfaces
{
	public interface IJobDetail
	{
		DateTime? Created { get; set; }

		Guid JobId { get; set; }

		string Detail { get; set; }
	}
}
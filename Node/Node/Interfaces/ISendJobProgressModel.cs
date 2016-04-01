using System;

namespace Stardust.Node.Interfaces
{
	public interface ISendJobProgressModel
	{
		DateTime? Created { get; set; }

		Guid JobId { get; set; }

		string ProgressDetail { get; set; }
	}
}
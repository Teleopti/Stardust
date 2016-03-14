using System;
using System.Net.Http;

namespace Stardust.Node.Interfaces
{
	public interface ISendJobProgressModel
	{
		DateTime Created { get; set; }

		Guid JobId { get; set; }
		string ProgressDetail { get; set; }

		HttpResponseMessage ResponseMessage { get; set; }
	}
}
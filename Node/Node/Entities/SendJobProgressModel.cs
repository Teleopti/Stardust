using System;
using System.Net.Http;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Entities
{
	public class SendJobProgressModel : ISendJobProgressModel
	{
		public Guid JobId { get; set; }

		public string ProgressDetail { get; set; }

		public DateTime Created { get; set; }

		public HttpResponseMessage ResponseMessage { get; set; }
	}
}
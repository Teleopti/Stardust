using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class ReplyRequestsInput
	{
		public IEnumerable<Guid> RequestIds { get; set; }
		public string Message { get; set; }
	}
}
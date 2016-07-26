using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class RequestsCommandInput
	{
		public IEnumerable<Guid> SelectedRequestIds { get; set; }
		public string ReplyMessage { get; set; }
	}
}
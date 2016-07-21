using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.FormData
{
	public class RequestsCommandInput
	{
		public IEnumerable<Guid> RequestIds { get; set; }
		public string ReplyMessage { get; set; }
	}
}
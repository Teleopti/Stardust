using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Portal
{
	public class DirectLineToken
	{
		public string conversationId { get; set; }
		public string token { get; set; }
		public int expires_in { get; set; }
	}

	public class GrantBotConfig
	{
		public string Token { get; set; }
		public string Timestamp { get; set; }
		public string Signature { get; set; }
	}
}
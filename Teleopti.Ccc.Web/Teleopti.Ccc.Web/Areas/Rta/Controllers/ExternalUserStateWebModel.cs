using System;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class ExternalUserStateWebModel
	{
		public ExternalUserStateWebModel()
		{
			Timestamp = DateTime.Now.ToString();
		}

		public string AuthenticationKey { get; set; }
		public string PlatformTypeId { get; set; }
		public string SourceId { get; set; }

		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public bool IsLoggedOn { get; set; }
		public int SecondsInState { get; set; }
		public string Timestamp { get; set; }
		public string BatchId { get; set; }
		public bool IsSnapshot { get; set; }
	}
}
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.States
{
	public class ExternalUserStateWebModel
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
	}

	public class ExternalUserBatchWebModel
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public bool IsSnapshot { get; set; }
		public IEnumerable<ExternalUserBatchStateWebModel> States { get; set; }
	}

	public class ExternalUserBatchStateWebModel
	{
		public string UserCode { get; set; }
		public string StateCode { get; set; }
	}
	
}
﻿namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ExternalUserStateWebModel
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string SnapshotId { get; set; }
	}
}
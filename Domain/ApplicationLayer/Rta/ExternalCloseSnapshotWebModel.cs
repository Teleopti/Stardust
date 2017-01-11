using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ExternalCloseSnapshotWebModel
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public DateTime SnapshotId { get; set; }
	}
}
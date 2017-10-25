using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class InputInfo
	{
		public string SourceId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public DateTime? SnapshotId { get; set; }
		public int? SnapshotDataSourceId { get; set; }
		
		// for logging
		public override string ToString() => $"SourceId: {SourceId}, UserCode: {UserCode}, StateCode: {StateCode}, StateDescription: {StateDescription}, SnapshotId: {SnapshotId}, SnapshotDataSourceId: {SnapshotDataSourceId}";
	}
}
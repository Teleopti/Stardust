using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class BatchInputModel
	{
		public string AuthenticationKey { get; set; }
		public string SourceId { get; set; }
		public DateTime? SnapshotId { get; set; }
		public bool CloseSnapshot { get; set; }
		public IEnumerable<BatchStateInputModel> States { get; set; }

		// for logging
		public override string ToString() => $"AuthenticationKey: {AuthenticationKey}, SourceId: {SourceId}, SnapshotId: {SnapshotId}, States: {States.StringJoin(x => x.ToString(), ",")}";
	}

	public class BatchStateInputModel
	{
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public StateTraceLog TraceLog { get; set; }

		// for logging
		public override string ToString() => $"UserCode: {UserCode}, StateCode: {StateCode}, StateDescription: {StateDescription}";
	}
}
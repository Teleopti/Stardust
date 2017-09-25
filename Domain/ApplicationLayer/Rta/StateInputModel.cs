using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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
		public override string ToString()
		{
			var states =
				States.EmptyIfNull().Any()
					? States.EmptyIfNull()
						.Select(x => x.ToString())
						.Aggregate((current, next) => current + ", " + next)
					: "";
			return $"AuthenticationKey: {AuthenticationKey}, SourceId: {SourceId}, SnapshotId: {SnapshotId}, States: {states}";
		}

	}

	public class BatchStateInputModel
	{
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public StateTraceInfo TraceInfo { get; set; }

		// for logging
		public override string ToString()
		{
			return $"UserCode: {UserCode}, StateCode: {StateCode}, StateDescription: {StateDescription}";
		}

	}

}
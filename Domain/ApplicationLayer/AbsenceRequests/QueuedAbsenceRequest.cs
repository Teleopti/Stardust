using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class QueuedAbsenceRequest
	{
		public QueuedAbsenceRequest(Guid personRequestId, DateTime created, DateTime starDateTime, DateTime endDateTime, Guid businessUnitId)
		{
			PersonRequestId = personRequestId;
			Created = created;
			StarDateTime = starDateTime;
			EndDateTime = endDateTime;
			BusinessUnitId = businessUnitId;
		}

		public Guid PersonRequestId { get; }

		public DateTime Created { get; }

		public DateTime StarDateTime { get; }

		public DateTime EndDateTime { get; }

		public Guid BusinessUnitId { get; }
	}
}
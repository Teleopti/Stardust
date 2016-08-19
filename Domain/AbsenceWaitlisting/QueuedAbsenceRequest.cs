using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	public class QueuedAbsenceRequest : NonversionedAggregateRootWithBusinessUnit, IQueuedAbsenceRequest
	{
		public virtual Guid PersonRequest { get; set; }

		public virtual DateTime Created { get; set; }

		public virtual DateTime StartDateTime { get; set; }

		public virtual DateTime EndDateTime { get; set; }

	}
}
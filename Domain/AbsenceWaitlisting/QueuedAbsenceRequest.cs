using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	public class QueuedAbsenceRequest : AggregateRoot_Events_ChangeInfo_BusinessUnit, IQueuedAbsenceRequest
	{
		public virtual Guid PersonRequest { get; set; }

		public virtual DateTime Created { get; set; }

		public virtual DateTime StartDateTime { get; set; }

		public virtual DateTime EndDateTime { get; set; }

		public virtual DateTime? Sent { get; set; }

		public virtual RequestValidatorsFlag MandatoryValidators { get; set; }
	}
}
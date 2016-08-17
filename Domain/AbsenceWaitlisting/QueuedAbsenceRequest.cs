using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	public class QueuedAbsenceRequest : Entity, IQueuedAbsenceRequest
	{
		public virtual IPersonRequest PersonRequest { get; set; }
		public virtual DateTime Created { get; set; }
		public virtual DateTime StartDateTime { get; set; }
		public virtual DateTime EndDateTime { get; set; }
		public virtual IBusinessUnit BusinessUnit { get; set; }
	}
}
using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AbsenceWaitlisting
{
	public class QueuedAbsenceRequest : Entity, IQueuedAbsenceRequest
	{
		private IPersonRequest _personRequest;
		private DateTime _created;
		private DateTime _startDateTime;
		private DateTime _endDateTime;
		private IBusinessUnit _businessUnit;

		public virtual IPersonRequest PersonRequest
		{
			get { return _personRequest; }
			set { _personRequest = value; }
		}

		public virtual DateTime Created
		{
			get { return _created; }
			set { _created = value; }
		}

		public virtual DateTime StartDateTime
		{
			get { return _startDateTime; }
			set { _startDateTime = value; }
		}

		public virtual DateTime EndDateTime
		{
			get { return _endDateTime; }
			set { _endDateTime = value; }
		}

		public virtual IBusinessUnit BusinessUnit
		{
			get { return _businessUnit; }
			set { _businessUnit = value; }
		}
	}
}
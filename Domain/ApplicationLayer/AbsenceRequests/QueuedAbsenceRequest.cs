using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class QueuedAbsenceRequest : IQueuedAbsenceRequest
	{

		public bool Equals(IEntity other)
		{
			throw new NotImplementedException();
		}
		
		public void SetId(Guid? newId)
		{
			throw new NotImplementedException();
		}

		public void ClearId()
		{
			throw new NotImplementedException();
		}

		public Guid? Id { get; }
		public IPersonRequest PersonRequest { get; set; }
		public DateTime Created { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public IBusinessUnit BusinessUnit { get; set; }
	}
}
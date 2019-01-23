using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;

namespace Teleopti.Ccc.Domain.Repositories
{

	public class PersonWaitlistedAbsenceRequest
	{
		public Guid PersonRequestId { get; set; }
		public PersonRequestStatus RequestStatus { get; set; }
	}
}

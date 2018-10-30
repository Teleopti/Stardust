using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy
{
	public class AbsenceRequestModel
	{
		public string Subject;
		public DateTimePeriod Period;
		public string Message;
		public Guid AbsenceId;
		public Guid PersonId;
		public bool FullDay;
		public Guid? PersonRequestId;
		public bool IsNew => !PersonRequestId.HasValue;
	}
}
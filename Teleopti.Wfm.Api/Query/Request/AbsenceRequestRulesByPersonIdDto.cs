using System;

namespace Teleopti.Wfm.Api.Query.Request
{
	public class AbsenceRequestRulesByPersonIdDto : IQueryDto
	{
		public Guid PersonId;
		public DateTime StartDate;
		public DateTime EndDate;
	}
}
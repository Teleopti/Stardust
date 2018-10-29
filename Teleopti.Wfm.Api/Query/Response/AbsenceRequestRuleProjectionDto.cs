using System;

namespace Teleopti.Wfm.Api.Query.Response
{
	public class AbsenceRequestRuleProjectionDto
	{
		public DateTime StartDate;
		public DateTime EndDate;
		public string RequestProcess;
		public string PersonAccountValidator;
		public string StaffingThresholdValidator;
	}
}
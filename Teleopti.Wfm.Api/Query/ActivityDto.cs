using System;

namespace Teleopti.Wfm.Api.Query
{
	public class ActivityDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public bool InReadyTime { get; set; }
		public bool RequiresSkill { get; set; }
		public bool InWorkTime { get; set; }
		public bool InPaidTime { get; set; }
		public string ReportLevelDetail { get; set; }
		public bool RequiresSeat { get; set; }
		public string PayrollCode { get; set; }
		public bool AllowOverwrite { get; set; }
		public bool IsOutboundActivity { get; set; }
	}
}
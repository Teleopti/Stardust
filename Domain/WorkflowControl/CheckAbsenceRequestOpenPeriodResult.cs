namespace Teleopti.Ccc.Domain.WorkflowControl
{
	internal class CheckAbsenceRequestOpenPeriodResult
	{
		internal string DenyReason { get; set; }

		internal bool HasSuggestedPeriod { get; set; }
	}
}

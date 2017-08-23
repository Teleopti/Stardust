namespace Teleopti.Ccc.Domain.Reports
{
	public interface IReportVisible
	{
		string ForeignId();
	}

	public class ReportScheduledTimePerActivityVisible : IReportVisible
	{
		public string ForeignId()
		{
			return "0055";
		}
	}

	public class ReportScheduledTimeVsTargetVisible : IReportVisible
	{
		public string ForeignId()
		{
			return "0064";
		}
	}

	public class ReportAuditTrailVisible : IReportVisible
	{
		public string ForeignId()
		{
			return "0059";
		}
	}
}
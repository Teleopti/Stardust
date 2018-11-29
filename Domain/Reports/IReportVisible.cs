namespace Teleopti.Ccc.Domain.Reports
{
	public interface IReportVisible
	{
		string ForeignId();
	}

	public class ReportScheduledTimeVsTargetVisible : IReportVisible
	{
		public string ForeignId()
		{
			return "0064";
		}
	}
}
namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport
{
    public class DailyMetricsViewModel
	{
		public string DisplayDate { get; set; }
		public string Adherence { get; set; }
		public string Readiness { get; set; }
		public string AverageHandlingTime { get; set; }
		public string AverageTalkTime { get; set; }
		public string AverageAfterWork { get; set; }
		public int AnsweredCalls { get; set; }
    }
}
namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport
{
    public class QueueMetricsViewModel
    {
        public string Queue { get; set; }
        public string AverageHandlingTime { get; set; }
        public string AverageTalkTime { get; set; }
        public string AverageAfterCallWork { get; set; }
        public int AnsweredCalls { get; set; }
        public bool DataAvailable { get; set; }
    }
}
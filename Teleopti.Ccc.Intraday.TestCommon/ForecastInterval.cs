namespace Teleopti.Ccc.Intraday.TestCommon
{
	public class ForecastInterval
	{
		public int DateId { get; set; }
		public int IntervalId { get; set; }
		public double Calls { get; set; }
		public double HandleTime { get; set; }
	    public double TalkTime { get; set; }
	    public double AfterTalkTime { get; set; }
	}
}
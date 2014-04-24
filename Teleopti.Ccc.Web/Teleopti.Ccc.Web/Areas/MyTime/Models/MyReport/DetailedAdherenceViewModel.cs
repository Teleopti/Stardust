using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport
{
	public class DetailedAdherenceViewModel
	{
		public string ShiftDate { get; set; }
		public string TotalAdherence { get; set; }
		public int IntervalsPerDay { get; set; }
		public ICollection<AdherenceIntervalViewModel> Intervals { get; set; }
		public bool DataAvailable { get; set; }
	}

	public class AdherenceIntervalViewModel
	{
		public int IntervalId { get; set; }
		public double Adherence { get; set; }
		public int IntervalCounter { get; set; }
		public double Deviation { get; set; }
		public string Name { get; set; }
		public string Color { get; set; }
	}
}
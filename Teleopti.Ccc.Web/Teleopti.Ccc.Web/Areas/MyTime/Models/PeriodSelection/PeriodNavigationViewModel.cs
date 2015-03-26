using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection
{
	public class PeriodNavigationViewModel
	{
		public string NextPeriod { get; set;}
		public bool HasNextPeriod { get; set; }
		public string PrevPeriod { get; set; }
		public bool HasPrevPeriod { get; set; }
		public bool CanPickPeriod { get; set; }
	}
}
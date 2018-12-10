using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.WebReports
{
	public class DetailedAdherenceForDayResult
	{
		public DateOnly ShiftDate { get; set; }
		public Percent TotalAdherence { get; set; }
		public int IntervalsPerDay { get; set; }
		public int IntervalId { get; set; }
		public int IntervalCounter { get; set; }
		public double Adherence { get; set; }
		public int Deviation { get; set; }
		public Color DisplayColor { get; set; }
		public string DisplayName { get; set; }
	}
}
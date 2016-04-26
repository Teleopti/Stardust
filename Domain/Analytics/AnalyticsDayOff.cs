using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsDayOff
	{
		public int DayOffId { get; set; }
		public Guid DayOffCode { get; set; }
		public string DayOffName { get; set; }
		public int BusinessUnitId { get; set; }
		public string DayOffShortname { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public int DisplayColor { get; set; }
		public string DisplayColorHtml { get; set; }
	}
}
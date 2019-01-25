using System;

namespace Teleopti.Ccc.Domain.Intraday.To_Staffing
{
	public class SkillForecast
	{
		public DateTime StartDateTime { get; set; }
		public Guid SkillId { get; set; }
		public double Agents { get; set; }
		public double Calls { get; set; }
		public double AverageHandleTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}
}

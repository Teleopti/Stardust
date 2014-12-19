using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventLayer
	{
		public ProjectionChangedEventLayer()
		{
			Name = "";
			ShortName = "";
		}

		public Guid PayloadId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public TimeSpan WorkTime { get; set; }
		public TimeSpan ContractTime { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string PayrollCode { get; set; }
		public int DisplayColor { get; set; }
		public bool IsAbsence { get; set; }
		public bool RequiresSeat { get; set; }
		public bool IsAbsenceConfidential { get; set; }
		public TimeSpan Overtime { get; set; }
		public Guid MultiplicatorDefinitionSetId { get; set; }
	}
}
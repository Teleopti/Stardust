using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	/// <summary>
	/// A denormalized schedule day
	/// </summary>
	public class ProjectionChangedEventScheduleDay
	{
		/// <summary>
		/// Team id
		/// </summary>
		public Guid TeamId { get; set; }
		/// <summary>
		/// Site id
		/// </summary>
		public Guid SiteId { get; set; }
		
		/// <summary>
		/// Date
		/// </summary>
		public DateTime Date { get; set; }
		/// <summary>
		/// The work time
		/// </summary>
		public TimeSpan WorkTime { get; set; }
		/// <summary>
		/// The contract time
		/// </summary>
		public TimeSpan ContractTime { get; set; }
		/// <summary>
		/// The label
		/// </summary>
		public string ShortName { get; set; }
		public string Name { get; set; }
		/// <summary>
		/// The color
		/// </summary>
		public int DisplayColor { get; set; }
		/// <summary>
		/// Is this a work day
		/// </summary>
		public bool IsWorkday { get; set; }

		public bool IsFullDayAbsence { get; set; }

		public bool NotScheduled { get; set; }

		public ProjectionChangedEventDayOff DayOff { get; set; }

		public ProjectionChangedEventShift Shift { get; set; }
	}
}
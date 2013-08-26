using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	/// <summary>
	/// A denormalized shift layer
	/// </summary>
	public class ProjectionChangedEventLayer
	{
        /// <summary>
		/// The payload id
		/// </summary>
		public Guid PayloadId { get; set; }
		/// <summary>
		/// The layer start time
		/// </summary>
		public DateTime StartDateTime { get; set; }
		/// <summary>
		/// The layer end time
		/// </summary>
		public DateTime EndDateTime { get; set; }
		/// <summary>
		/// The layer work time
		/// </summary>
		public TimeSpan WorkTime { get; set; }
		/// <summary>
		/// The layer contract time
		/// </summary>
		public TimeSpan ContractTime { get; set; }
		/// <summary>
		/// The name of the payload
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// The short name of the payload
		/// </summary>
		public string ShortName { get; set; }
		/// <summary>
		/// The payroll code of the payload
		/// </summary>
		public string PayrollCode { get; set; }
		/// <summary>
		/// The display color of the payload
		/// </summary>
		public int DisplayColor { get; set; }
		/// <summary>
		/// Is this absence
		/// </summary>
		public bool IsAbsence { get; set; }
		/// <summary>
		/// Requires seat
		/// </summary>
		public bool RequiresSeat { get; set; }
	}
}
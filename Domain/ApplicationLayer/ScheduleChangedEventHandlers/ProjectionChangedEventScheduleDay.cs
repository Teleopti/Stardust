﻿using System;
using System.Collections.Generic;

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
		public string Label { get; set; }
		/// <summary>
		/// The color
		/// </summary>
		public int DisplayColor { get; set; }
		/// <summary>
		/// Is this a work day
		/// </summary>
		public bool IsWorkday { get; set; }
		/// <summary>
		/// Optional shift start
		/// </summary>
		public DateTime? StartDateTime { get; set; }
		/// <summary>
		/// Optional shift end
		/// </summary>
		public DateTime? EndDateTime { get; set; }

		/// <summary>
		/// The layers for this shift
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public ICollection<ProjectionChangedEventLayer> Layers { get; set; }
	}
}